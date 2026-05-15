import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import CartPageContent from '@/features/cart/CartPageContent';

type Props = { params: Promise<{ locale: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'cart' });
  return { title: t('title') };
}

export default async function CartPage({ params }: Props) {
  const { locale } = await params;
  return <CartPageContent locale={locale} />;
}
