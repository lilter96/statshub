import React from 'react';
import { useRevenueStats } from '../../hooks/useRevenueStats';
import { RevenueChart } from '../RevenueChart/RevenueChart';
import { Loader } from '../Loader/Loader';
import './Home.css';

export const Home = () => {
    const { stats, loading, error } = useRevenueStats({
        apiBaseUrl: '',
        hubUrl: "https://localhost:7065",
    });

    if (loading) return <Loader message="Loading chart…" />;
    if (error) return <div className="error">Error: {error.message}</div>;

    return (
        <main className="revenue-page">
            <h1 className="revenue-title">📈 Daily Revenue</h1>
            <div className="revenue-chart-wrapper">
                <RevenueChart stats={stats} />
            </div>
        </main>
    );
};
