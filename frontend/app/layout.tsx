import type { Metadata } from 'next';
import { headers } from 'next/headers';
import './globals.css';

const siteUrl = process.env.PUBLIC_SITE_URL?.trim() || 'http://localhost:3000';

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: { default: 'Servis Binjaket', template: '%s · Servis Binjaket' },
  description: 'Servis Binjaket — elektronikë dhe shërbime riparimi në Durrës',
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const localeHeader = (await headers()).get('x-next-intl-locale');
  const language = localeHeader === 'en' ? 'en' : 'sq';

  return (
    <html lang={language}>
      <body>{children}</body>
    </html>
  );
}
