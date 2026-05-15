'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useCart } from '@/features/cart/CartContext';
import type { ProductDetail } from '@/lib/types';

interface Props {
  product: ProductDetail;
  locale: string;
}

export default function AddToCartButton({ product, locale }: Props) {
  const t = useTranslations('productDetails');
  const { addItem } = useCart();
  const [added, setAdded] = useState(false);

  function handleAdd() {
    addItem({
      productId: product.id,
      slug: product.slug,
      name: product.name,
      price: product.price,
      currency: product.currency,
      image: product.image,
      quantity: 1,
    });
    setAdded(true);
  }

  if (!product.inStock) {
    return (
      <div className="w-full py-3 px-6 bg-muted text-muted-foreground rounded-xl font-semibold text-center">
        {t('outOfStock')}
      </div>
    );
  }

  if (added) {
    return (
      <Link
        href={`/${locale}/cart`}
        className="inline-flex items-center justify-center w-full py-3 px-6 bg-emerald-600 text-white rounded-xl font-semibold hover:bg-emerald-700 transition-colors"
      >
        {t('addedToCart')}
      </Link>
    );
  }

  return (
    <button
      onClick={handleAdd}
      className="inline-flex items-center justify-center gap-2 w-full py-3 px-6 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors"
    >
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
          d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
      </svg>
      {t('addToCart')}
    </button>
  );
}
