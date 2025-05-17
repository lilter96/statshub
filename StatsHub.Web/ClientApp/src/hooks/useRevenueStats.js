import { useEffect, useRef, useState, useCallback } from 'react';
import { HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr';

export const useRevenueStats = ({ apiBaseUrl, hubUrl }) => {
    const [stats, setStats] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const connRef = useRef(null);

    const fetchInitial = useCallback(async () => {
        try {
            const res = await fetch(`${apiBaseUrl}/orders/daily-stats`, { credentials: 'include' });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            setStats(data);
        } catch (err) {
            setError(err);
        }
    }, [apiBaseUrl]);

    useEffect(() => {
        let canceled = false;

        (async () => {
            await fetchInitial();

            const conn = new HubConnectionBuilder()
                .withUrl(`${hubUrl}/hubs/revenue`, {
                    skipNegotiation: true,
                    transport: HttpTransportType.WebSockets,
                })
                .configureLogging(LogLevel.Information)
                .withAutomaticReconnect([0, 1000, 3000, 5000])
                .build();

            conn.on('update', payload => {
                if (!canceled) setStats(payload);
            });

            try {
                await conn.start();
            } catch (err) {
                console.error('SignalR start failed:', err);
                setError(err);
            } finally {
                if (!canceled) setLoading(false);
            }

            connRef.current = conn;
        })();

        return () => {
            canceled = true;
            connRef.current?.stop();
        };
    }, [fetchInitial, hubUrl]);

    return { stats, loading, error };
};
