import React from 'react';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
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

        let createdAt;
        if (!stats.length) {
            createdAt = new Date().toISOString();
        } else {
            const rnd = stats[Math.floor(Math.random() * stats.length)].date;
            const dt = new Date(rnd);
            dt.setDate(dt.getDate() + 5);
            createdAt = dt.toISOString();
        }

        const revenues = stats.map(x => x.revenue);
        const maxRevenue = revenues.length ? Math.max(...revenues) : 0;
        const price = maxRevenue ? Math.ceil(maxRevenue * 0.25) : 1000;

        const newOrder = {
            orderId: crypto.randomUUID(),
            sku: 'SKU-' + Math.floor(Math.random() * 10000),
            price,
            quantity: 1,
            createdAt,
            brandName: 'TestBrand'
        };

        try {
            const res = await fetch('/orders/sync', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify([newOrder])
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            toast.success(
                `Добавлено 1 заказ на ${new Date(createdAt).toLocaleDateString()}. Смотрите график!`,
                { autoClose: 5000 }
            );
        } catch (err) {
            console.error('Sync failed:', err);
            toast.error('Не удалось добавить заказ');
        }
    };

    if (loading) return <Loader message="Loading chart…" />;
    if (error) return <div className="error">Error: {error.message}</div>;

    return (
        <>
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
            <ToastContainer />
        </>
    );
};
