import React from 'react';

export const Loader = ({ message }) => (
    <div className="loader-overlay">
        <div className="spinner" />
        <p>{message}</p>
    </div>
);
