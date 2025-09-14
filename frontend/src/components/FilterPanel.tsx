import React from 'react';
import { SubscriptionFilter } from '../types/Bond';

interface FilterPanelProps {
  filter: SubscriptionFilter;
  onChange: (filter: SubscriptionFilter) => void;
}

const FilterPanel: React.FC<FilterPanelProps> = ({ filter, onChange }) => {
  const currencyOptions = ['EUR', 'GBP', 'USD'];
  const sectorOptions = ['Government', 'Corporate', 'Municipal'];

  const handleCurrencyChange = (currency: string) => {
    const newCurrencies = filter.currencies.includes(currency)
      ? filter.currencies.filter(c => c !== currency)
      : [...filter.currencies, currency];

    onChange({
      ...filter,
      currencies: newCurrencies
    });
  };

  const handleSectorChange = (sector: string) => {
    const newSectors = filter.sectors.includes(sector)
      ? filter.sectors.filter(s => s !== sector)
      : [...filter.sectors, sector];

    onChange({
      ...filter,
      sectors: newSectors
    });
  };

  const clearFilters = () => {
    onChange({
      currencies: [],
      sectors: []
    });
  };

  return (
    <div style={{ padding: '16px', background: '#f9f9f9', marginBottom: '16px' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '12px' }}>
        <h3 style={{ margin: '0' }}>Filters</h3>
        <button 
          onClick={clearFilters}
          style={{
            padding: '4px 12px',
            border: '1px solid #ccc',
            borderRadius: '4px',
            background: '#f44336',
            color: 'white',
            cursor: 'pointer',
            fontSize: '12px'
          }}
        >
          Clear All Filters
        </button>
      </div>

      <div style={{ display: 'flex', gap: '32px' }}>
        <div>
          <strong>Currency:</strong>
          <div style={{ display: 'flex', gap: '8px', marginTop: '4px' }}>
            {currencyOptions.map(currency => (
              <label key={currency} style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <input
                  type="checkbox"
                  checked={filter.currencies.includes(currency)}
                  onChange={() => handleCurrencyChange(currency)}
                />
                {currency}
              </label>
            ))}
          </div>
        </div>

        <div>
          <strong>Sector:</strong>
          <div style={{ display: 'flex', gap: '8px', marginTop: '4px' }}>
            {sectorOptions.map(sector => (
              <label key={sector} style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <input
                  type="checkbox"
                  checked={filter.sectors.includes(sector)}
                  onChange={() => handleSectorChange(sector)}
                />
                {sector}
              </label>
            ))}
          </div>
        </div>
      </div>

      {(filter.currencies.length > 0 || filter.sectors.length > 0) && (
        <div style={{ marginTop: '12px', fontSize: '12px', color: '#666' }}>
          <strong>Active Filters:</strong>
          {filter.currencies.length > 0 && ` Currencies: ${filter.currencies.join(', ')}`}
          {filter.sectors.length > 0 && ` Sectors: ${filter.sectors.join(', ')}`}
        </div>
      )}
    </div>
  );
};

export default FilterPanel;