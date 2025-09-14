import React from 'react';

interface GroupingSelectorProps {
  options: string[];
  selectedGroups: string[];
  onChange: (groups: string[]) => void;
}

const GroupingSelector: React.FC<GroupingSelectorProps> = ({ 
  options, 
  selectedGroups, 
  onChange 
}) => {
  const handleGroupToggle = (groupCol: string) => {
    const currentIndex = selectedGroups.indexOf(groupCol);
    let newGroups: string[] = [];

    if (currentIndex === -1) {
      newGroups = [...selectedGroups, groupCol];
    } else {
      newGroups = selectedGroups.filter(g => g !== groupCol);
    }

    onChange(newGroups);
  };

  const moveUp = (index: number) => {
    if (index > 0) {
      const newGroups = [...selectedGroups];
      [newGroups[index - 1], newGroups[index]] = [newGroups[index], newGroups[index - 1]];
      onChange(newGroups);
    }
  };

  const moveDown = (index: number) => {
    if (index < selectedGroups.length - 1) {
      const newGroups = [...selectedGroups];
      [newGroups[index], newGroups[index + 1]] = [newGroups[index + 1], newGroups[index]];
      onChange(newGroups);
    }
  };

  const clearAll = () => {
    onChange([]);
  };

  return (
    <div style={{ padding: '16px', background: '#f5f5f5', marginBottom: '16px' }}>
      <h3 style={{ margin: '0 0 12px 0' }}>Grouping Configuration</h3>
      
      <div style={{ marginBottom: '12px' }}>
        <strong>Available Columns:</strong>
        <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap', marginTop: '4px' }}>
          {options.map(option => (
            <button
              key={option}
              onClick={() => handleGroupToggle(option)}
              style={{
                padding: '4px 8px',
                border: '1px solid #ccc',
                borderRadius: '4px',
                background: selectedGroups.includes(option) ? '#007acc' : 'white',
                color: selectedGroups.includes(option) ? 'white' : 'black',
                cursor: 'pointer'
              }}
            >
              {option.charAt(0).toUpperCase() + option.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {selectedGroups.length > 0 && (
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '8px' }}>
            <strong>Group By (in order):</strong>
            <button 
              onClick={clearAll}
              style={{
                padding: '2px 8px',
                border: '1px solid #ccc',
                borderRadius: '4px',
                background: '#f44336',
                color: 'white',
                cursor: 'pointer',
                fontSize: '12px'
              }}
            >
              Clear All
            </button>
          </div>
          
          {selectedGroups.map((group, index) => (
            <div 
              key={group} 
              style={{ 
                display: 'flex', 
                alignItems: 'center', 
                gap: '8px',
                padding: '4px 8px',
                background: 'white',
                border: '1px solid #ddd',
                borderRadius: '4px',
                marginBottom: '4px'
              }}
            >
              <span style={{ flex: 1 }}>
                {index + 1}. {group.charAt(0).toUpperCase() + group.slice(1)}
              </span>
              <button
                onClick={() => moveUp(index)}
                disabled={index === 0}
                style={{
                  padding: '2px 6px',
                  border: '1px solid #ccc',
                  borderRadius: '2px',
                  background: index === 0 ? '#f0f0f0' : 'white',
                  cursor: index === 0 ? 'not-allowed' : 'pointer',
                  fontSize: '12px'
                }}
              >
                ↑
              </button>
              <button
                onClick={() => moveDown(index)}
                disabled={index === selectedGroups.length - 1}
                style={{
                  padding: '2px 6px',
                  border: '1px solid #ccc',
                  borderRadius: '2px',
                  background: index === selectedGroups.length - 1 ? '#f0f0f0' : 'white',
                  cursor: index === selectedGroups.length - 1 ? 'not-allowed' : 'pointer',
                  fontSize: '12px'
                }}
              >
                ↓
              </button>
              <button
                onClick={() => handleGroupToggle(group)}
                style={{
                  padding: '2px 6px',
                  border: '1px solid #ccc',
                  borderRadius: '2px',
                  background: '#f44336',
                  color: 'white',
                  cursor: 'pointer',
                  fontSize: '12px'
                }}
              >
                ✕
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default GroupingSelector;