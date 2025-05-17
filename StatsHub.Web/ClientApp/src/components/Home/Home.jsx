import React from 'react';
import { useRevenueStats } from '../../hooks/useRevenueStats';
import { RevenueChart } from '../RevenueChart/RevenueChart';
import { Loader } from '../Loader/Loader';
import './Home.css';

export const Home = () => {
    const { stats, loading, error } = useRevenueStats({
        apiBaseUrl: '',
        hubUrl: 'https://localhost:7065',
    });

    const handleTryIt = async () => {
        if (!stats.length) return;

        const randomDay = stats[Math.floor(Math.random() * stats.length)].date;

        const newOrder = {
            orderId: crypto.randomUUID(),
            sku: 'SKU-' + Math.floor(Math.random() * 10000),
            price: 1000,
            quantity: 1,
            createdAt: randomDay,
            brandName: 'TestBrand'
        };

        try {
            const res = await fetch('/orders/sync', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify([newOrder])
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        } catch (err) {
            console.error('Sync failed:', err);
            alert('Не удалось добавить заказ');
        }
    };

    if (loading) return <Loader message="Loading chart…" />;
    if (error) return <div className="error">Error: {error.message}</div>;

    return (
        <main className="revenue-page">
            <h1 className="revenue-title">📈 Daily Revenue</h1>
            <div className="revenue-chart-wrapper">
                <RevenueChart stats={stats} />
            </div>

            <section className="try-section">
                <p className="try-text">
                    Добавляйте новые заказы — они появятся на графике в реальном времени!
                </p>
                <button className="try-button" onClick={handleTryIt}>
                    Try It
                </button>
            </section>
        </main>
    );
};
