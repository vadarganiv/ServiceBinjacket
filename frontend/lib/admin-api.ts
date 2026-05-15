const API_URL =
  process.env.API_URL ??
  process.env.NEXT_PUBLIC_API_URL ??
  'http://localhost:5000';

export async function adminApiFetch<T>(
  path: string,
  options?: RequestInit
): Promise<T> {
  const res = await fetch(`${API_URL}/api/v1${path}`, {
    ...options,
    credentials: 'include',
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

export async function adminServerFetch<T>(
  path: string,
  cookieHeader: string,
  options?: RequestInit
): Promise<{ data: T | null; status: number }> {
  const res = await fetch(`${API_URL}/api/v1${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Cookie: cookieHeader,
      ...options?.headers,
    },
    cache: 'no-store',
  });
  if (!res.ok) return { data: null, status: res.status };
  const data = await res.json();
  return { data, status: res.status };
}
