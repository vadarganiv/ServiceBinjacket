export const API_URL = process.env.API_URL ?? 'http://localhost:5000';

// Normalizes Albanian local numbers (069xxxxxxx) to international format (355...)
export function toWhatsAppNumber(phone: string): string {
  const digits = phone.replace(/\D/g, '');
  if (digits.startsWith('00')) return digits.slice(2);
  if (digits.length === 10 && digits.startsWith('0')) return '355' + digits.slice(1);
  return digits;
}

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
