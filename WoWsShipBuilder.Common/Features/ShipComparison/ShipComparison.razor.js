
export function exportTableToExcel(section) {
    let table = document.getElementsByClassName("mud-table-root")[0];
    TableToExcel.convert(table, {
        name: `wowssb-ship-comparison-grid.xlsx`,
        sheet: {
            name: section,
        }
    });
}
