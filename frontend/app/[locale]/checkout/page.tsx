import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import CheckoutForm from '@/features/orders/CheckoutForm';

type Props = { params: Promise<{ locale: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'checkout' });
  return { title: t('title'), description: t('metaDescription') };
}

export default async function CheckoutPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'checkout' });

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-foreground mb-8">{t('title')}</h1>
      <CheckoutForm locale={locale} />
    </div>
  );
}
