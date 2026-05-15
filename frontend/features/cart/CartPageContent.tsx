'use client';

import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useCart } from './CartContext';
import CartItem from './CartItem';

interface Props {
  locale: string;
}

export default function CartPageContent({ locale }: Props) {
  const t = useTranslations('cart');
  const tProducts = useTranslations('products');
  const { items, subtotal } = useCart();

  if (items.length === 0) {
    return (
      <div className="container mx-auto px-4 py-20">
        <div className="flex flex-col items-center text-center max-w-sm mx-auto">
          <div className="w-20 h-20 rounded-full bg-muted flex items-center justify-center mb-6">
            <svg className="w-10 h-10 text-muted-foreground opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
          </div>
          <h1 className="text-2xl font-bold mb-2">{t('empty')}</h1>
          <p className="text-muted-foreground mb-8">{t('emptyHint')}</p>
          <Link href={`/${locale}/products`} className="inline-flex items-center justify-center px-6 py-3 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors">
            {t('browseProducts')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-2xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold">{t('title')}</h1>
          <Link href={`/${locale}/products`} className="text-sm text-muted-foreground hover:text-foreground transition-colors">
            ← {t('continueShopping')}
          </Link>
        </div>

        <div className="border border-border rounded-2xl p-6 mb-6">
          {items.map(item => (
            <CartItem key={item.productId} item={item} />
          ))}
        </div>

        <div className="border border-border rounded-2xl p-6 space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground">{t('subtotal')}</span>
            <span className="text-2xl font-bold text-primary">
              {subtotal.toLocaleString()} {tProducts('currency')}
            </span>
          </div>
          <Link
            href={`/${locale}/checkout`}
            className="flex items-center justify-center w-full py-3 px-6 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors"
          >
            {t('checkout')}
          </Link>
        </div>
      </div>
    </div>
  );
}
