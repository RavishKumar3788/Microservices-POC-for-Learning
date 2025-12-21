import React, { useState, useEffect } from 'react';
import './PlaceOrder.css';

interface Product {
    id: string;
    name: string;
    price: number;
    description?: string;
}

interface PlaceOrderProps {
    product: Product;
    userId: string;
    onOrderPlaced?: (orderId: string) => void;
    onCancel?: () => void;
}

const PlaceOrder: React.FC<PlaceOrderProps> = ({
    product,
    userId,
    onOrderPlaced,
    onCancel,
}) => {
    const [quantity, setQuantity] = useState<number>(1);
    const [totalPrice, setTotalPrice] = useState<number>(product.price);
    const [isLoading, setIsLoading] = useState<boolean>(false);

    useEffect(() => {
        setTotalPrice(product.price * quantity);
    }, [quantity, product.price]);

    const handleQuantityChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = parseInt(e.target.value);
        if (value > 0) {
            setQuantity(value);
        }
    };

    const handlePlaceOrder = async () => {
        setIsLoading(true);
        try {
            // TODO: Replace with actual API call
            const orderId = `ORDER-${Date.now()}`;
            console.log('Order placed:', { userId, product, quantity, totalPrice });

            if (onOrderPlaced) {
                onOrderPlaced(orderId);
            }
        } catch (error) {
            console.error('Error placing order:', error);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="place-order-container">
            <div className="place-order-card">
                <h2 className="order-title">Place Order</h2>

                <div className="order-details">
                    <div className="product-info">
                        <h3 className="product-name">{product.name}</h3>
                        {product.description && (
                            <p className="product-description">{product.description}</p>
                        )}
                        <p className="product-price">Unit Price: ${product.price.toFixed(2)}</p>
                    </div>

                    <div className="form-group">
                        <label htmlFor="userId" className="form-label">
                            User ID
                        </label>
                        <input
                            type="text"
                            id="userId"
                            value={userId}
                            disabled
                            className="form-input disabled"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="quantity" className="form-label">
                            Quantity
                        </label>
                        <input
                            type="number"
                            id="quantity"
                            min="1"
                            value={quantity}
                            onChange={handleQuantityChange}
                            className="form-input"
                        />
                    </div>

                    <div className="total-section">
                        <span className="total-label">Total Price:</span>
                        <span className="total-amount">${totalPrice.toFixed(2)}</span>
                    </div>
                </div>

                <div className="button-group">
                    <button
                        onClick={handlePlaceOrder}
                        disabled={isLoading}
                        className="btn btn-primary"
                    >
                        {isLoading ? 'Placing Order...' : 'Place Order'}
                    </button>
                    {onCancel && (
                        <button onClick={onCancel} className="btn btn-secondary">
                            Cancel
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default PlaceOrder;