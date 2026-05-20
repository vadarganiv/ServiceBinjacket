import type { NextConfig } from 'next';
import createNextIntlPlugin from 'next-intl/plugin';

const withNextIntl = createNextIntlPlugin('./i18n/request.ts');

const allowedApiOrigins = new Set([
  'http://backend:5000',
  'http://localhost:5000',
  'http://127.0.0.1:5000',
]);

function resolveApiUrl() {
  const fallbackApiUrl = process.env.NODE_ENV === 'production'
    ? 'http://backend:5000'
    : 'http://localhost:5000';
  const rawApiUrl = process.env.API_URL ?? fallbackApiUrl;

  let parsedApiUrl: URL;
  try {
    parsedApiUrl = new URL(rawApiUrl);
  } catch {
    throw new Error('Invalid API_URL: expected an absolute internal backend origin');
  }

  if (parsedApiUrl.pathname !== '/' || parsedApiUrl.search || parsedApiUrl.hash) {
    throw new Error('Invalid API_URL: path, query and fragment are not allowed');
  }

  const apiOrigin = parsedApiUrl.origin;
  if (!allowedApiOrigins.has(apiOrigin)) {
    throw new Error(`Invalid API_URL origin: ${apiOrigin}`);
  }

  return apiOrigin;
}

const apiUrl = resolveApiUrl();

const nextConfig: NextConfig = {
  output: 'standalone',
  async rewrites() {
    return [
      { source: '/api/:path*', destination: `${apiUrl}/api/:path*` },
      { source: '/uploads/:path*', destination: `${apiUrl}/uploads/:path*` },
    ];
  },
};

export default withNextIntl(nextConfig);
