const SERVER_BASE = process.env.API_URL ?? 'http://localhost:5000';

function apiBase() {
  return typeof window === 'undefined' ? `${SERVER_BASE}/api/v1` : '/api/v1';
}

export async function apiFetch<T>(
  path: string,
  options?: RequestInit
): Promise<T> {
  const res = await fetch(`${apiBase()}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  });
  if (!res.ok) {
    throw new Error(`API error ${res.status}: ${path}`);
  }
  return res.json();
}
