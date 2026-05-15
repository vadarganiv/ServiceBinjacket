'use client';

import { useTranslations } from 'next-intl';
import { useCart } from './CartContext';
import type { CartItem as CartItemType } from '@/lib/types';

interface Props {
  item: CartItemType;
}

export default function CartItem({ item }: Props) {
  const t = useTranslations('cart');
  const tProducts = useTranslations('products');
  const { removeItem, updateQuantity } = useCart();

  return (
    <div className="flex gap-4 py-4 border-b border-border last:border-0">
      <div className="w-20 h-20 flex-shrink-0 rounded-xl bg-muted overflow-hidden">
        {item.image ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={item.image.path} alt={item.image.alt} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <svg className="w-8 h-8 text-muted-foreground opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}
      </div>

      <div className="flex flex-1 flex-col gap-2 min-w-0">
        <p className="text-sm font-semibold text-foreground leading-snug line-clamp-2">{item.name}</p>
        <p className="text-sm font-bold text-primary">
          {(item.price * item.quantity).toLocaleString()} {tProducts('currency')}
        </p>
        <div className="flex items-center gap-3">
          <div className="flex items-center border border-border rounded-lg overflow-hidden">
            <button
              onClick={() => updateQuantity(item.productId, item.quantity - 1)}
              className="w-8 h-8 flex items-center justify-center hover:bg-muted transition-colors text-foreground"
              aria-label="Decrease quantity"
            >
              −
            </button>
            <span className="w-8 text-center text-sm font-medium">{item.quantity}</span>
            <button
              onClick={() => updateQuantity(item.productId, item.quantity + 1)}
              className="w-8 h-8 flex items-center justify-center hover:bg-muted transition-colors text-foreground"
              aria-label="Increase quantity"
            >
              +
            </button>
          </div>
          <button
            onClick={() => removeItem(item.productId)}
            className="text-xs text-muted-foreground hover:text-red-500 transition-colors"
          >
            {t('remove')}
          </button>
        </div>
      </div>

      <div className="text-right text-xs text-muted-foreground whitespace-nowrap">
        {item.price.toLocaleString()} × {item.quantity}
      </div>
    </div>
  );
}
