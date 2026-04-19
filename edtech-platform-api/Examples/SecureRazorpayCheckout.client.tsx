"use client";

/**
 * Razorpay + httpOnly cookie auth + CSRF for Next.js (App Router).
 * Env: NEXT_PUBLIC_API_URL (must match an entry in API Cors:AllowedOrigins).
 * Do not store JWT in localStorage.
 */

import Script from "next/script";
import { useCallback, useRef, useState } from "react";

const apiBase = () => process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "";

type CreateOrderResponse = {
  orderId: string;
  amount: number;
  currency: string;
  keyId: string;
  gateway: string;
  courseTitle: string;
  paymentId: number;
};

declare global {
  interface Window {
    Razorpay: new (options: Record<string, unknown>) => { open: () => void };
  }
}

async function fetchCsrf(): Promise<string> {
  const res = await fetch(`${apiBase()}/api/auth/csrf`, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) throw new Error("Not authenticated or CSRF preflight failed");
  const data = (await res.json()) as { csrfToken: string };
  if (!data?.csrfToken) throw new Error("Invalid CSRF response");
  return data.csrfToken;
}

async function postJson<T>(path: string, body: unknown, csrf: string): Promise<T> {
  const res = await fetch(`${apiBase()}${path}`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      "X-CSRF-TOKEN": csrf,
    },
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { error?: string }).error ?? `HTTP ${res.status}`);
  }
  return res.json() as Promise<T>;
}

export function SecureRazorpayCheckout(props: {
  courseId: number;
  batchId: number;
  onSuccess?: () => void;
}) {
  const { courseId, batchId, onSuccess } = props;
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const scriptReady = useRef(false);

  const runCheckout = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      if (!apiBase()) throw new Error("Set NEXT_PUBLIC_API_URL");

      const csrf1 = await fetchCsrf();
      const order = await postJson<CreateOrderResponse>(
        "/api/Payment/create-order",
        { courseId, batchId },
        csrf1
      );

      if (!scriptReady.current || typeof window.Razorpay === "undefined") {
        throw new Error("Razorpay script still loading — try again");
      }

      const amountPaise = Math.round(Number(order.amount) * 100);

      const rzp = new window.Razorpay({
        key: order.keyId,
        amount: amountPaise,
        currency: order.currency,
        order_id: order.orderId,
        name: order.courseTitle,
        description: "Course enrollment",
        handler: async (response: {
          razorpay_payment_id: string;
          razorpay_order_id: string;
          razorpay_signature: string;
        }) => {
          try {
            const csrf2 = await fetchCsrf();
            await postJson("/api/Payment/verify", {
              razorpayOrderId: response.razorpay_order_id,
              razorpayPaymentId: response.razorpay_payment_id,
              razorpaySignature: response.razorpay_signature,
              batchId,
            }, csrf2);
            onSuccess?.();
          } catch (e) {
            setError(e instanceof Error ? e.message : "Verification failed");
          }
        },
      });

      rzp.open();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Payment failed");
    } finally {
      setLoading(false);
    }
  }, [batchId, courseId, onSuccess]);

  return (
    <>
      <Script
        src="https://checkout.razorpay.com/v1/checkout.js"
        strategy="lazyOnload"
        onLoad={() => {
          scriptReady.current = true;
        }}
      />
      <button type="button" onClick={() => void runCheckout()} disabled={loading}>
        {loading ? "Processing…" : "Pay securely"}
      </button>
      {error ? <p role="alert">{error}</p> : null}
    </>
  );
}
