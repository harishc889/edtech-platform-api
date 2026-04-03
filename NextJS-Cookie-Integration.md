# Next.js Cookie Authentication Integration Guide

## 🔐 Cookie Security Features Enabled

- **HttpOnly**: ✅ Prevents JavaScript/XSS access to cookies
- **Secure**: ✅ HTTPS only (production) / HTTP allowed (development)
- **SameSite**: ✅ Strict CSRF protection
- **Credentials**: ✅ Allows cookies in cross-origin requests
- **Expiry**: ✅ 7 days (matches JWT expiry)

---

## 📦 Next.js Setup

### 1. Install Dependencies

```bash
npm install axios
# or
npm install ky
```

### 2. Create API Client (`lib/api.ts`)

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  withCredentials: true, // Enable cookies
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;
```

### 3. Environment Variables (`.env.local`)

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### 4. Auth Service (`services/authService.ts`)

```typescript
import api from '@/lib/api';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  name: string;
  email: string;
  password: string;
}

export const authService = {
  // Login - Cookie automatically set by backend
  async login(credentials: LoginCredentials) {
    const response = await api.post('/api/auth/login', credentials);
    return response.data;
  },

  // Register
  async register(data: RegisterData) {
    const response = await api.post('/api/auth/register', data);
    return response.data;
  },

  // Logout - Cookie automatically cleared by backend
  async logout() {
    const response = await api.post('/api/auth/logout');
    return response.data;
  },

  // Get current user
  async me() {
    const response = await api.get('/api/user/me');
    return response.data;
  },
};
```

### 5. Login Page (`app/login/page.tsx`)

```typescript
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/authService';

export default function LoginPage() {
  const router = useRouter();
  const [credentials, setCredentials] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.login(credentials);
      console.log('Login successful:', response.message);
      
      // Redirect to dashboard
      router.push('/dashboard');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center">
      <form onSubmit={handleSubmit} className="space-y-4 w-full max-w-md">
        <h1 className="text-2xl font-bold">Login</h1>
        
        {error && (
          <div className="bg-red-100 text-red-700 p-3 rounded">
            {error}
          </div>
        )}

        <input
          type="email"
          placeholder="Email"
          value={credentials.email}
          onChange={(e) => setCredentials({ ...credentials, email: e.target.value })}
          required
          className="w-full p-2 border rounded"
        />

        <input
          type="password"
          placeholder="Password"
          value={credentials.password}
          onChange={(e) => setCredentials({ ...credentials, password: e.target.value })}
          required
          className="w-full p-2 border rounded"
        />

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-500 text-white p-2 rounded hover:bg-blue-600"
        >
          {loading ? 'Logging in...' : 'Login'}
        </button>
      </form>
    </div>
  );
}
```

### 6. Protected Route Middleware (`middleware.ts`)

```typescript
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const authToken = request.cookies.get('auth_token');

  // Protected routes
  const protectedPaths = ['/dashboard', '/courses', '/profile'];
  const isProtectedPath = protectedPaths.some(path => 
    request.nextUrl.pathname.startsWith(path)
  );

  // Redirect to login if accessing protected route without token
  if (isProtectedPath && !authToken) {
    const url = request.nextUrl.clone();
    url.pathname = '/login';
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/dashboard/:path*', '/courses/:path*', '/profile/:path*'],
};
```

### 7. Dashboard Page (`app/dashboard/page.tsx`)

```typescript
'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/authService';

export default function DashboardPage() {
  const router = useRouter();
  const [user, setUser] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchUser();
  }, []);

  const fetchUser = async () => {
    try {
      const data = await authService.me();
      setUser(data);
    } catch (error) {
      console.error('Failed to fetch user:', error);
      router.push('/login');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      await authService.logout();
      router.push('/login');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="min-h-screen p-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold">Dashboard</h1>
          <button
            onClick={handleLogout}
            className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
          >
            Logout
          </button>
        </div>

        <div className="bg-white p-6 rounded shadow">
          <h2 className="text-xl font-semibold mb-4">Welcome!</h2>
          <p>Email: {user?.email}</p>
          <p>Name: {user?.name}</p>
        </div>
      </div>
    </div>
  );
}
```

### 8. Fetch My Enrolled Courses

```typescript
// services/courseService.ts
import api from '@/lib/api';

export const courseService = {
  // Get my enrolled courses
  async getMyEnrolledCourses() {
    const response = await api.get('/api/enroll/my-courses');
    return response.data;
  },

  // Get all published courses
  async getPublishedCourses() {
    const response = await api.get('/api/course');
    return response.data;
  },

  // Enroll in a batch
  async enrollInBatch(batchId: number) {
    const response = await api.post('/api/enroll', { batchId });
    return response.data;
  },
};
```

---

## 🔄 How It Works

### Login Flow:
```
1. User submits login form → POST /api/auth/login
2. Backend validates credentials
3. Backend generates JWT token
4. Backend sets HttpOnly cookie: auth_token
5. Next.js receives response (cookie set automatically)
6. User redirected to dashboard
7. All subsequent requests include cookie automatically
```

### API Request Flow:
```
1. Next.js makes request with withCredentials: true
2. Browser automatically includes auth_token cookie
3. Backend reads token from cookie (OnMessageReceived event)
4. Backend validates JWT and authorizes request
5. Response sent back to Next.js
```

---

## 🔒 Security Best Practices Implemented

1. ✅ **HttpOnly Cookies** - XSS protection
2. ✅ **Secure Flag** - HTTPS only in production
3. ✅ **SameSite=Strict** - CSRF protection
4. ✅ **CORS with Credentials** - Only allow specific origins
5. ✅ **Token Expiry** - 7-day expiration
6. ✅ **Session Management** - Max 2 active sessions per user

---

## 🧪 Testing

### Test Login:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password123"}' \
  -c cookies.txt

# Check cookies.txt - you should see auth_token
```

### Test Protected Endpoint:
```bash
curl -X GET http://localhost:5000/api/enroll/my-courses \
  -b cookies.txt
```

---

## 📝 Environment-Specific Settings

### Development (HTTP):
- Secure: false
- SameSite: Lax
- Domain: localhost

### Production (HTTPS):
- Secure: true
- SameSite: Strict
- Domain: yourdomain.com

Update your production settings to use environment variables!

---

## 🚀 Ready to Use!

Your API now supports:
- ✅ Cookie-based authentication
- ✅ Bearer token authentication (backward compatible)
- ✅ Next.js integration
- ✅ Mobile app support (can still use Bearer tokens)
- ✅ Best security practices
