import {useEffect, useRef, useState} from 'react';
import {HubConnectionBuilder, LogLevel} from '@microsoft/signalr';
import {Line} from 'react-chartjs-2';
import {
    CategoryScale,
    Chart as ChartJS,
    Legend,
    LinearScale,
    LineElement,
    PointElement,
    TimeScale,
    Tooltip
} from 'chart.js';
import './Home.css';

ChartJS.register(LineElement, PointElement, LinearScale, TimeScale, Tooltip, Legend, CategoryScale);

export function Home() {
    const [stats, setStats] = useState([]);
    const [loading, setLoading] = useState(true);
    const connRef = useRef(null);

    const split = (arr) => ({
        labels: arr.map((x) => x.date),
        revenues: arr.map((x) => x.revenue)
    });

    useEffect(() => {
        (async () => {
            try {
                // initial data fetch
                const res = await fetch('/orders/daily-stats');
                const initial = await res.json();
                setStats(initial);
            } catch (err) {
                console.error('Initial fetch failed:', err);
            }

            const conn = new HubConnectionBuilder()
                .withUrl('https://localhost:7065/hubs/revenue')
                .configureLogging(LogLevel.Trace)
                .withAutomaticReconnect([0, 1000, 3000, 5000])
                .build();

            conn.on('update', (payload) => {
                setStats([...payload]);
            });

            try {
                await conn.start();
                console.log('SignalR connected');
                setLoading(false);
            } catch (err) {
                console.error('SignalR connection error:', err);
                setLoading(false);
            }
            connRef.current = conn;
        })();

        return () => {
            connRef.current?.stop();
        };
    }, []);

    if (loading) {
        return (
            <div className="loader-overlay">
                <div className="loader"/>
                <p>Loading chart...</p>
            </div>
        );
    }

    const {labels, revenues} = split(stats);
    const data = {
        labels,
        datasets: [
            {
                label: 'Revenue, ₽',
                data: revenues,
                tension: 0.4,
                borderColor: '#ffffff',
                backgroundColor: 'rgba(255, 255, 255, 0.2)',
                pointBackgroundColor: '#ffffff'
            }
        ]
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false
    };

    return (
        <main className="revenue-page">
            <h1 className="revenue-title">📈 Daily Revenue</h1>
            <div className="revenue-chart-wrapper">
                <Line data={data} options={options} redraw/>
            </div>
        </main>
    );
}
