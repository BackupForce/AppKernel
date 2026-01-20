# 前端：Refresh Token / Session 管理（Codex 指南）

> 目標：在瀏覽器端使用 **短效 Access Token + 長效 Refresh Token (Cookie)** 的登入流程，並支援 rotation 與 reuse detection。所有時間皆為 UTC。

## 0. 前置說明

- 後端預設 **Refresh Token 用 HttpOnly Secure Cookie** 儲存（`AuthTokenOptions.UseRefreshTokenCookie = true`）。
- 若後端切換為 **Body 模式**（`UseRefreshTokenCookie = false`），才需要在前端保存 refresh token；否則 **不應儲存在 localStorage**。

## 1) 登入流程（/auth/login）

### 1.1 修改/新增 API client
**檔案建議**：`src/api/auth.ts`

```ts
export interface LoginRequest {
  email: string;
  password: string;
  tenantCode: string;
  deviceId?: string;
}

export interface AuthTokenResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken?: string | null; // cookie 模式可忽略
  sessionId: string;
}

export async function login(payload: LoginRequest): Promise<AuthTokenResponse> {
  const response = await fetch('/api/v1/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include', // 讓瀏覽器收/送 refresh cookie
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw await response.json();
  }

  return response.json();
}
```

### 1.2 登入成功後的處理
**檔案建議**：`src/stores/auth.ts`

```ts
import { login } from '@/api/auth';

let accessToken: string | null = null;
let accessTokenExpiresAtUtc: string | null = null;

export async function loginAndStore(payload: LoginRequest) {
  const result = await login(payload);

  // Access Token 僅存 memory，避免落地。
  accessToken = result.accessToken;
  accessTokenExpiresAtUtc = result.accessTokenExpiresAtUtc;
}
```

> **注意**：cookie 模式下 `refreshToken` 會是 `null`，請不要嘗試存取。

## 2) Refresh Token 續期（/auth/refresh）

### 2.1 新增 refresh API
**檔案建議**：`src/api/auth.ts`

```ts
export async function refresh(): Promise<AuthTokenResponse> {
  const response = await fetch('/api/v1/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    // cookie 模式可不送 body
  });

  if (!response.ok) {
    throw await response.json();
  }

  return response.json();
}
```

### 2.2 Single-flight（避免多重 refresh）
**檔案建議**：`src/stores/auth.ts`

```ts
let refreshPromise: Promise<AuthTokenResponse> | null = null;

export async function refreshTokenSingleFlight() {
  if (!refreshPromise) {
    refreshPromise = refresh()
      .then((result) => {
        accessToken = result.accessToken;
        accessTokenExpiresAtUtc = result.accessTokenExpiresAtUtc;
        return result;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}
```

## 3) Axios / Fetch 攔截器（401/403 觸發 refresh）

### 3.1 Axios 範例
**檔案建議**：`src/api/http.ts`

```ts
import axios from 'axios';
import { refreshTokenSingleFlight } from '@/stores/auth';

const api = axios.create({
  baseURL: '/api/v1',
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error.response?.status;
    const errorCode = error.response?.data?.errorCode;

    if (status === 401 || status === 403) {
      if (errorCode === 'refresh_token_reused' || errorCode === 'session_revoked') {
        // 強制導回登入
        redirectToLogin();
        return Promise.reject(error);
      }

      try {
        await refreshTokenSingleFlight();
        return api.request(error.config);
      } catch (refreshError) {
        redirectToLogin();
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

### 3.2 Fetch 範例（簡化）
**檔案建議**：`src/api/fetcher.ts`

```ts
export async function fetchWithAuth(input: RequestInfo, init: RequestInit = {}) {
  const headers = new Headers(init.headers);
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(input, {
    ...init,
    headers,
    credentials: 'include',
  });

  if (response.status === 401 || response.status === 403) {
    const body = await response.clone().json().catch(() => null);
    const errorCode = body?.errorCode;

    if (errorCode === 'refresh_token_reused' || errorCode === 'session_revoked') {
      redirectToLogin();
      throw body ?? new Error('Unauthorized');
    }

    await refreshTokenSingleFlight();
    return fetch(input, { ...init, headers, credentials: 'include' });
  }

  return response;
}
```

## 4) 登出流程

### 4.1 登出目前裝置（/auth/logout）
**檔案建議**：`src/api/auth.ts`

```ts
export async function logout() {
  await fetch('/api/v1/auth/logout', {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
  });

  accessToken = null;
  accessTokenExpiresAtUtc = null;
}
```

### 4.2 登出全部裝置（/auth/logout-all）
```ts
export async function logoutAll() {
  await fetch('/api/v1/auth/logout-all', {
    method: 'POST',
    credentials: 'include',
  });

  accessToken = null;
  accessTokenExpiresAtUtc = null;
}
```

## 5) Session 管理（裝置列表）

### 5.1 取得 sessions
```ts
export async function getSessions() {
  const response = await fetch('/api/v1/auth/sessions', {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) {
    throw await response.json();
  }

  return response.json();
}
```

### 5.2 踢除指定裝置
```ts
export async function revokeSession(sessionId: string) {
  const response = await fetch(`/api/v1/auth/sessions/${sessionId}`, {
    method: 'DELETE',
    credentials: 'include',
  });

  if (!response.ok) {
    throw await response.json();
  }
}
```

## 6) Token 存放策略

- ✅ **Access Token**：只放 memory（例如 store、in-memory variable）。
- ❌ **不要**放 localStorage / sessionStorage。
- ✅ **Refresh Token**：由 HttpOnly Secure Cookie 管理（前端不可讀）。

## 7) 切換為 Body 模式（後端設定）

若後端設定：

```json
"AuthTokenOptions": {
  "UseRefreshTokenCookie": false
}
```

前端需調整：
1. **登入/刷新** API 的 response 會包含 `refreshToken`，請暫存在 memory（不落地）。
2. **/auth/refresh** 呼叫時需在 body 帶入 `{ "refreshToken": "..." }`。

## 8) 必須導回登入的狀況

- `refresh_token_reused`
- `session_revoked`

這兩種錯誤代表 session 已被撤銷或偵測重放，必須導回登入頁重新登入。
