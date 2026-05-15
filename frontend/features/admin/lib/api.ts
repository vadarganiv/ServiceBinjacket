export const API_URL =
  typeof window === 'undefined'
    ? (process.env.API_URL ?? 'http://localhost:5000')
    : (process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000');

export function adminHeaders(token?: string): HeadersInit {
  return token ? { Cookie: `sb_admin_token=${token}` } : {};
}

export async function adminFetch(path: string, token: string, options?: RequestInit) {
  const res = await fetch(`${API_URL}/api/v1${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Cookie: `sb_admin_token=${token}`,
      ...(options?.headers ?? {}),
    },
    cache: 'no-store',
  });
  return res;
}
