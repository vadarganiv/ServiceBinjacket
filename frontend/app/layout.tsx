import type { Metadata } from 'next';
import './globals.css';

const siteUrl = process.env.PUBLIC_SITE_URL?.trim() || 'http://localhost:3000';

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: { default: 'Servis Binjaket', template: '%s · Servis Binjaket' },
  description: 'Servis Binjaket — elektronikë dhe shërbime riparimi në Durrës',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html suppressHydrationWarning>
      <body>{children}</body>
    </html>
  );
}
