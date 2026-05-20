import createMiddleware from 'next-intl/middleware';
import { NextRequest, NextResponse } from 'next/server';
import { routing } from './i18n/routing';

const intlMiddleware = createMiddleware(routing);
const SAFE_PAGE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS']);

export default function middleware(req: NextRequest) {
  const { pathname } = req.nextUrl;

  if (!SAFE_PAGE_METHODS.has(req.method)) {
    return new NextResponse(null, {
      status: 405,
      headers: { Allow: 'GET, HEAD, OPTIONS' },
    });
  }

  if (pathname.startsWith('/admin')) {
    return NextResponse.next();
  }

  return intlMiddleware(req);
}

export const config = {
  matcher: ['/', '/(sq|en)/:path*', '/((?!api|_next|_vercel|.*\\..*).*)']
};
