import React, { useMemo } from 'react';
import { Line } from 'react-chartjs-2';


import {Chart as ChartJS, LinearScale, LineElement, PointElement, TimeScale, Tooltip, Legend, CategoryScale} from "chart.js";

ChartJS.register(LineElement, PointElement, LinearScale, TimeScale, Tooltip, Legend, CategoryScale);

export const RevenueChart = ({ stats }) => {
    const { labels, revenues } = useMemo(
        () => ({
            labels: stats.map(x => x.date),
            revenues: stats.map(x => x.revenue),
        }),
        [stats]
    );

    const data = useMemo(
        () => ({
            labels,
            datasets: [
                {
                    label: 'Revenue, ₽',
                    data: revenues,
                    tension: 0.3,
                    borderColor: '#4ade80',
                    backgroundColor: 'rgba(74, 222, 128, 0.2)',
                    pointRadius: 3,
                    pointHoverRadius: 5,
                },
            ],
        }),
        [labels, revenues]
    );

    const options = useMemo(
        () => ({
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { labels: { color: '#eee', font: { size: 14 } } },
                tooltip: { backgroundColor: '#333', titleColor: '#fff', bodyColor: '#ddd' },
            },
            scales: {
                x: { grid: { color: '#444' }, ticks: { color: '#bbb' } },
                y: { grid: { color: '#444' }, ticks: { color: '#bbb' } },
            },
        }),
        []
    );

    return <Line data={data} options={options} redraw />;
};
