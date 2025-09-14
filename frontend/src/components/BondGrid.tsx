import React, { useEffect, useRef, useState, useCallback, useMemo } from 'react';
import { AgGridReact } from 'ag-grid-react';
import { 
  ColDef, 
  GridApi, 
  IServerSideDatasource, 
  IServerSideGetRowsParams,
  GetRowIdParams,
  IsServerSideGroupOpenByDefaultParams
} from 'ag-grid-community';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';
import 'ag-grid-enterprise';
import { LicenseManager } from 'ag-grid-enterprise';
import { signalRService } from '../services/SignalRService';
import { Bond, ServerSideRequest, SubscriptionFilter } from '../types/Bond';

interface BondGridProps {
  groupingCols: string[];
  filterValues: SubscriptionFilter;
}

// Set AG Grid license for development/trial use
LicenseManager.setLicenseKey('[TRIAL]_this_{AG_Charts_and_AG_Grid}_Enterprise_key_{AG-063578}_is_granted_for_evaluation_only___Use_in_production_is_not_permitted___Please_report_misuse_to_legal@ag-grid.com___For_help_with_purchasing_a_production_license_key_please_contact_info@ag-grid.com___You_are_granted_a_{Single_Application}_Developer_License_for_one_application_only___All_Front-End_JavaScript_files_that_contain_AG_Grid_Enterprise_features_must_be_deployed_to_a_single_domain_only___This_key_will_deactivate_on_{29 October 2024}____[v3]_[01]_MTczMDA4NjQwMDAwMA==5b7b5bc8e36040c5b8e29bd7a6cf1b9e');

const BondGrid: React.FC<BondGridProps> = ({ groupingCols, filterValues }) => {
  const gridRef = useRef<AgGridReact>(null);
  const [gridApi, setGridApi] = useState<GridApi | null>(null);

  const defaultColDef:ColDef = useMemo(() => {
    return {
    resizable: true,
    sortable: true,
    wrapText: true,
    filter: true,
    minWidth: 100,
    maxWidth: 200,
    enableCellChangeFlash: true
  };
}, []);

  const columnDefs: ColDef[] = useMemo(() => [
    {
      field: 'instrumentId',
      headerName: 'Instrument ID',
      cellRenderer: 'agGroupCellRenderer',
      showRowGroup: true,
      minWidth: 200
    },
    {
      field: 'name',
      headerName: 'Name',
      minWidth: 200,
      filter: 'agTextColumnFilter'
    },
    {
      field: 'issuer',
      headerName: 'Issuer',
      minWidth: 150,
      filter: 'agTextColumnFilter'
    },
    {
      field: 'currency',
      headerName: 'Currency',
      width: 100,
      filter: 'agSetColumnFilter',
      filterParams: {
        values: (params: any) => {
          signalRService.getDistinctValues('currency')
            .then(values => {
              params.success(values);
            })
            .catch(error => {
              console.error('Failed to fetch currency values:', error);
              params.success([]);
            });
        }
      }
    },
    {
      field: 'sector',
      headerName: 'Sector',
      minWidth: 120,
      filter: 'agSetColumnFilter',
      filterParams: {
        values: (params: any) => {
          signalRService.getDistinctValues('sector')
            .then(values => {
              params.success(values);
            })
            .catch(error => {
              console.error('Failed to fetch sector values:', error);
              params.success([]);
            });
        }
      }
    },
    {
      field: 'maturityDate',
      headerName: 'Maturity',
      valueFormatter: (params) => {
        if (params.value) {
          return new Date(params.value).toLocaleDateString();
        }
        return '';
      },
      minWidth: 120
    },
    {
      field: 'couponRate',
      headerName: 'Coupon %',
      valueFormatter: (params) => params.value ? `${params.value.toFixed(2)}%` : '',
      width: 100
    },
    {
      field: 'bid',
      headerName: 'Bid',
      valueFormatter: (params) => params.value ? params.value.toFixed(3) : '',
      width: 80
    },
    {
      field: 'ask',
      headerName: 'Ask',
      valueFormatter: (params) => params.value ? params.value.toFixed(3) : '',
      width: 80
    },
    {
      field: 'spread',
      headerName: 'Spread',
      valueFormatter: (params) => params.value ? params.value.toFixed(3) : '',
      width: 80
    },
    {
      field: 'yield',
      headerName: 'Yield %',
      valueFormatter: (params) => params.value ? `${params.value.toFixed(2)}%` : '',
      width: 80
    },
    {
      field: 'lastPrice',
      headerName: 'Last',
      valueFormatter: (params) => params.value ? params.value.toFixed(3) : '',
      width: 80
    },
    {
      field: 'volume',
      headerName: 'Volume',
      valueFormatter: (params) => params.value ? params.value.toLocaleString() : '',
      width: 100
    },
    {
      field: 'rating',
      headerName: 'Rating',
      width: 80
    },
    {
      field: 'tierId',
      headerName: 'Tier',
      width: 80
    }
  ], []);

  const isServerSideGroup = useCallback((dataItem: any) => {
    return dataItem.isGroup === true;
  }, []);

  const getServerSideGroupKey = useCallback((dataItem: any) => {
    return dataItem.key || dataItem.instrumentId;
  }, []);

  const getRowId = useCallback((params: GetRowIdParams) => {
    if (params.data.key) {
      return params.data.key;
    }
    return params.data.instrumentId + (params.data.tierId ? '_' + params.data.tierId : '');
  }, []);

  const isServerSideGroupOpenByDefault = useCallback((params: IsServerSideGroupOpenByDefaultParams) => {
    return false;
  }, []);

  const createDatasource = useCallback((): IServerSideDatasource => {
    return {
      getRows: async (params: IServerSideGetRowsParams) => {
        try {
          // Merge AG Grid's filter model with our custom filter values
          const combinedFilterModel: any = { ...(params.request.filterModel || {}) };
          
          // Add our custom filter values
          if (filterValues.currencies.length > 0) {
            combinedFilterModel.currency = {
              filterType: 'set',
              values: filterValues.currencies
            };
          }
          
          if (filterValues.sectors.length > 0) {
            combinedFilterModel.sector = {
              filterType: 'set', 
              values: filterValues.sectors
            };
          }

          const request: ServerSideRequest = {
            startRow: params.request.startRow || 0,
            endRow: params.request.endRow || 100,
            sortModel: params.request.sortModel || [],
            filterModel: combinedFilterModel,
            groupKeys: params.request.groupKeys || [],
            groupingCols: groupingCols
          };

          const response = await signalRService.getBondRows(request);
          
          params.success({
            rowData: response.rows,
            rowCount: response.lastRow
          });
        } catch (error) {
          console.error('Error fetching rows:', error);
          
          // Provide empty data rather than failing completely
          params.success({
            rowData: [],
            rowCount: 0
          });
        }
      }
    };
  }, [groupingCols, filterValues]);

  const onGridReady = useCallback(async (params: any) => {
    setGridApi(params.api);
    
    try {
      console.log('Attempting to connect to SignalR...');
      await signalRService.connect();
      console.log('SignalR connected successfully');
      
      // Set up real-time update handlers before setting datasource
      signalRService.onUpdateBond((bond: Bond) => {
        params.api.applyServerSideTransaction({
          update: [bond]
        });
      });

      signalRService.onBatchUpdateBonds((bonds: Bond[]) => {
        if (bonds.length > 0) {
          params.api.applyServerSideTransaction({
            update: bonds
          });
        }
      });

      signalRService.onRefreshGroup((path: string[]) => {
        params.api.refreshServerSide({ route: path });
      });

      // Subscribe to filters first
      await signalRService.subscribeToFilter(filterValues);
      
      // Only set datasource after connection and subscription are complete
      params.api.setGridOption('serverSideDatasource', createDatasource());

    } catch (error) {
      console.error('Failed to connect to SignalR. Make sure the backend is running on http://localhost:5000');
      console.error('Error details:', error);
      
      // Still set up the datasource for grid functionality, but without real-time updates
      params.api.setGridOption('serverSideDatasource', createDatasource());
    }
  }, [createDatasource, filterValues]);

  useEffect(() => {
    const updateDatasource = async () => {
      if (gridApi) {
        // Wait for connection before updating datasource
        try {
          await signalRService.connect();
          gridApi.setGridOption('serverSideDatasource', createDatasource());
        } catch (error) {
          console.warn('SignalR not connected, setting datasource anyway');
          gridApi.setGridOption('serverSideDatasource', createDatasource());
        }
      }
    };
    
    updateDatasource();
  }, [gridApi, createDatasource]);

  useEffect(() => {
    const updateFilter = async () => {
      if (gridApi) {
        try {
          await signalRService.connect();
          await signalRService.subscribeToFilter(filterValues);
        } catch (error) {
          console.warn('Could not update filter subscription:', error);
        }
      }
    };
    
    updateFilter();
  }, [gridApi, filterValues]);

  useEffect(() => {
    return () => {
      signalRService.disconnect();
    };
  }, []);

  return (
    <div className="ag-theme-alpine" style={{ height: '600px', width: '100%' }}>
      <AgGridReact
        ref={gridRef}
        defaultColDef={defaultColDef}
        columnDefs={columnDefs}
        rowModelType="serverSide"
        treeData={true}
        animateRows={true}
        cacheBlockSize={100}
        maxBlocksInCache={10}
        onGridReady={onGridReady}
        isServerSideGroup={isServerSideGroup}
        getServerSideGroupKey={getServerSideGroupKey}
        getRowId={getRowId}
        isServerSideGroupOpenByDefault={isServerSideGroupOpenByDefault}
        suppressRowClickSelection={true}
        suppressCellFocus={true}
      />
    </div>
  );
};

export default BondGrid;