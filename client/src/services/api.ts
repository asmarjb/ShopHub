import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5169/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const getAnonymousCartId = (): string => {
  let cartId = localStorage.getItem('anonymous_cart_id');
  if (!cartId) {
    cartId = crypto.randomUUID();
    localStorage.setItem('anonymous_cart_id', cartId);
  }
  return cartId;
};

// Request Interceptor: Token & Anonymous Cart ID əlavə edilməsi
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  } else {
    config.headers['X-Anonymous-Cart-Id'] = getAnonymousCartId();
  }
  return config;
});

// Response Interceptor: 401 olduqda avtomatik Refresh Token axını
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: AxiosError | null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve();
    }
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => {
    if (response.data !== null && response.data !== undefined) {
      if (typeof response.data === 'object' && !('data' in response.data)) {
        try {
          Object.defineProperty(response.data, 'data', {
            value: response.data,
            writable: true,
            enumerable: false,
            configurable: true,
          });
        } catch {
          // ignore
        }
      }
    }
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (originalRequest.url?.includes('/auth/login') || originalRequest.url?.includes('/auth/refresh-token')) {
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => api(originalRequest))
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refresh_token');
      const accessToken = localStorage.getItem('access_token');

      if (!refreshToken) {
        isRefreshing = false;
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        return Promise.reject(error);
      }

      try {
        const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
          accessToken,
          refreshToken,
        });

        const data = response.data?.data || response.data;
        if (data?.accessToken) {
          localStorage.setItem('access_token', data.accessToken);
          localStorage.setItem('refresh_token', data.refreshToken);

          processQueue(null);
          return api(originalRequest);
        }
      } catch (refreshErr) {
        processQueue(refreshErr as AxiosError);
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        window.dispatchEvent(new Event('auth:logout'));
        return Promise.reject(refreshErr);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
