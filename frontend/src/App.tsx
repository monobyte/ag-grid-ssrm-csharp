import React, { useState } from 'react';
import BondGrid from './components/BondGrid';
import GroupingSelector from './components/GroupingSelector';
import FilterPanel from './components/FilterPanel';
import { SubscriptionFilter } from './types/Bond';

function App() {
  const [groupingCols, setGroupingCols] = useState<string[]>([]);
  const [filterValues, setFilterValues] = useState<SubscriptionFilter>({
    currencies: [],
    sectors: []
  });

  const groupingOptions = ['sector', 'currency', 'issuer', 'rating'];

  return (
    <div style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <div style={{ marginBottom: '20px' }}>
        <h1 style={{ margin: '0 0 8px 0', color: '#333' }}>
          Bond Trading Dashboard
        </h1>
        <p style={{ margin: '0', color: '#666', fontSize: '14px' }}>
          Real-time bond pricing with server-side row model and custom grouping
        </p>
      </div>

      <FilterPanel 
        filter={filterValues}
        onChange={setFilterValues}
      />

      <GroupingSelector
        options={groupingOptions}
        selectedGroups={groupingCols}
        onChange={setGroupingCols}
      />

      <div style={{ 
        border: '1px solid #ddd', 
        borderRadius: '8px',
        overflow: 'hidden',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
      }}>
        <BondGrid
          groupingCols={groupingCols}
          filterValues={filterValues}
        />
      </div>

      <div style={{ 
        marginTop: '16px', 
        padding: '12px', 
        background: '#f0f0f0', 
        borderRadius: '4px',
        fontSize: '12px',
        color: '#666'
      }}>
        <strong>Instructions:</strong>
        <ul style={{ margin: '4px 0', paddingLeft: '16px' }}>
          <li>Use filters to subscribe to specific currencies or sectors</li>
          <li>Configure grouping by selecting and ordering group columns</li>
          <li>Click on bond rows (with group icon) to expand and view tier pricing</li>
          <li>Prices update automatically in real-time via SignalR</li>
        </ul>
      </div>
    </div>
  );
}

export default App;