import { NextIntlClientProvider } from 'next-intl';
import { getMessages } from 'next-intl/server';
import { notFound } from 'next/navigation';
import { routing } from '@/i18n/routing';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import WhatsAppFab from '@/components/WhatsAppFab';
import { CartProvider } from '@/features/cart/CartContext';

type Props = {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
};

export default async function LocaleLayout({ children, params }: Props) {
  const { locale } = await params;

  if (!routing.locales.includes(locale as 'sq' | 'en')) {
    notFound();
  }

  const messages = await getMessages();
  const whatsappPhone = process.env.WHATSAPP_PHONE ?? '';

  return (
    <div lang={locale}>
      <NextIntlClientProvider messages={messages}>
        <CartProvider>
          <Header />
          <main className="min-h-screen">{children}</main>
          <Footer locale={locale} />
          <WhatsAppFab phone={whatsappPhone} locale={locale} />
        </CartProvider>
      </NextIntlClientProvider>
    </div>
  );
}
