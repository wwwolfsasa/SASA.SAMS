$(function () {
    //同一 格 變色
    $(document).on('mouseenter', '.asrs_wh_cell', function (e) {
        $('.asrs_wh_bay[bay=' + $(this).attr('bay') + ']').addClass('same_bay_highlight');
    }).on('mouseleave', '.asrs_wh_cell', function (e) {
        $('.asrs_wh_bay').removeClass('same_bay_highlight');
    });


    ASRS_Warehouse.GetCostumCellType();
});

var ASRS_Warehouse = {
    /**目前 列 數 
      *@type {number}
     */
    CurrentRow: null,
    /**Sync Cell State Timer */
    SyncCellStateTimer: null,
    /**初始化 */
    InitialCell: function () {
        //綁定回傳資料事件
        $('.asrs_wh_cell').bind(ASRS_Warehouse.Event.OnCellStateChange, function (e, data) {
            var bay = parseInt($(this).attr('bay')) - 1;
            var level = parseInt($(this).attr('level')) - 1;
            $(this).attr({
                'disable': (data[bay][level].Prohibit && data[bay][level].SystemProhibit) ? 'enable' : 'disable',
                'cell-type': (data[bay][level].CellType) ? data[bay][level].CellType : 'NAN',
                'cell-status': data[bay][level].CellStatus
            });

            if (ASRS_Warehouse.CELL_TYPE[data[bay][level].CellType] != undefined && ASRS_Warehouse.CELL_TYPE[data[bay][level].CellType].isCostum) {
                $(this).css({
                    border: '2px solid #{0}'.Format([ASRS_Warehouse.CELL_TYPE[data[bay][level].CellType].color])
                });
            }
        });
    },
    /** 取得所有 Cell 狀態*/
    GetCellState: function () {
        $.ajax({
            url: 'http://localhost:32000/km/warehouse/cell/get/' + ASRS_Warehouse.CurrentRow,
            type: 'POST',
            dataType: 'json',
            cache: false,
            success: function (resp) {
                //console.log(resp);
                if (resp.isSuccess) {
                    ASRS_Warehouse.SyncCellData = resp.Data;
                    $('.asrs_wh_cell').trigger(ASRS_Warehouse.Event.OnCellStateChange, [resp.Data])
                } else {
                    console.log(resp.Status);
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    },
    /**同步資料 */
    SyncCellData: null,
    /**同步 */
    SyncGetCellState: function () {
        this.SyncCellStateTimer = setInterval(function () {
            ASRS_Warehouse.GetCellState();
        }, 60 * 1000);
    },
    /**Cell Click 
     * @param bay {number}
     * @param level {number}
     */
    OnCellClick: function (bay, level) {
        bay -= 1;
        level -= 1;
        var thisCell = this.SyncCellData[bay][level];

        $('.cell_state').remove();

        var cell_state = $('<div></div>').addClass('cell_state').click(function () {
            $(this).remove();
        });

        var cell_state_frame = $('<table></table>').addClass('cell_state_frame').attr({
            border: 1
        }).css({
            position: 'absolute'
        }).click(function (e) {
            e.stopPropagation();
        });

        var cell_state_title = $('<caption></caption>').addClass('cell_state_title').text('儲位 {0}-{1}-{2}'.Format([thisCell.Row, thisCell.Bay, thisCell.Level]));

        var cell_state_tr_0 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_01 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_01 = $('<span></span>').addClass('cell_state_col').text('儲格種類');
        cell_state_td_01.html(cell_state_col_01);
        var cell_state_td_02 = $('<td></td>').addClass('cell_state_td').attr({
            colspan: 2
        });
        var cell_state_col_02 = this.CellTypeDropDownList(thisCell.CellType);
        cell_state_td_02.html(cell_state_col_02);
        cell_state_tr_0.append([cell_state_td_01, cell_state_td_02]);

        var cell_state_tr_1 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_11 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_11 = $('<span></span>').addClass('cell_state_col').text('儲格狀態');
        cell_state_td_11.html(cell_state_col_11);
        var cell_state_td_12 = $('<td></td>').addClass('cell_state_td').attr({
            colspan: 2
        });
        var cell_state_col_12 = this.CellStatusDropDownList(thisCell.CellStatus);
        cell_state_td_12.html(cell_state_col_12);
        cell_state_tr_1.append([cell_state_td_11, cell_state_td_12]);

        var cell_state_tr_2 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_21 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_21 = $('<span></span>').addClass('cell_state_col').text('棧板號碼');
        cell_state_td_21.html(cell_state_col_21);
        var cell_state_td_22 = $('<td></td>').addClass('cell_state_td').attr({
            colspan: 2
        });
        var cell_state_col_22 = $('<input></input>').addClass('cell_state_col').attr({
            id: 'wh_cell_set_pallet',
            type: 'text'
        }).val(thisCell.PalletID);
        var cell_state_col_23 = $('<input></input>').addClass('cell_state_col').attr({
            id: 'btn_check_item_on_pallet',
            type: 'button'
        }).val('');
        cell_state_td_22.append([cell_state_col_22, cell_state_col_23]);
        cell_state_tr_2.append([cell_state_td_21, cell_state_td_22]);

        var cell_state_tr_3 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_31 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_31 = $('<span></span>').addClass('cell_state_col').text('啟用狀態');
        cell_state_td_31.html(cell_state_col_31);
        var cell_state_td_32 = $('<td></td>').addClass('cell_state_td').attr({
            colspan: 2
        });
        var cell_state_col_32 = $('<input></input>').addClass('cell_state_col').attr({
            type: 'checkbox'
        }).prop('checked', thisCell.Prohibit);
        cell_state_td_32.html(cell_state_col_32);
        cell_state_tr_3.append([cell_state_td_31, cell_state_td_32]);

        var cell_state_tr_4 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_41 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_41 = $('<span></span>').addClass('cell_state_col').text('系統啟用');
        cell_state_td_41.html(cell_state_col_41);
        var cell_state_td_42 = $('<td></td>').addClass('cell_state_td').attr({
            colspan: 2
        });
        var cell_state_col_42 = $('<input></input>').addClass('cell_state_col').attr({
            type: 'checkbox',
            readonly: 'readonly'
        }).prop('checked', thisCell.SystemProhibit);
        cell_state_td_42.html(cell_state_col_42);
        cell_state_tr_4.append([cell_state_td_41, cell_state_td_42]);

        var cell_state_tr_5 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_51 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_51 = $('<span></span>').addClass('cell_state_col').text('最後更新時間');
        cell_state_td_51.html(cell_state_col_51);
        var cell_state_td_52 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_52 = $('<input></input>').addClass('cell_state_col').attr({
            type: 'text',
            readonly: 'readonly'
        }).val(new Date(thisCell.UpdateTime).Format('yyyy-MM-dd'));
        cell_state_td_52.html(cell_state_col_52);
        var cell_state_td_53 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_53 = $('<input></input>').addClass('cell_state_col').attr({
            type: 'text',
            readonly: 'readonly'
        }).timepicker({
            'timeFormat': 'H:i:s'
        }).val(new Date(thisCell.UpdateTime).Format('HH:mm:ss'));
        cell_state_td_53.html(cell_state_col_53);
        cell_state_tr_5.append([cell_state_td_51, cell_state_td_52, cell_state_td_53]);

        var cell_state_tr_6 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_61 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_61 = $('<span></span>').addClass('cell_state_col').text('最後保存時間');
        cell_state_td_61.html(cell_state_col_61);
        var cell_state_td_62 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_62 = $('<input></input>').addClass('cell_state_col').attr({
            type: 'text'
        }).datepicker({
            dateFormat: 'yy-mm-dd'
        }).val(new Date(thisCell.StoreDeadline).Format('yyyy-MM-dd'));
        cell_state_td_62.html(cell_state_col_62);
        var cell_state_td_63 = $('<td></td>').addClass('cell_state_td');
        var cell_state_col_63 = $('<input></input>').addClass('cell_state_col').attr({
            type: 'text'
        }).timepicker({
            'timeFormat': 'H:i:s'
        }).val(new Date(thisCell.StoreDeadline).Format('HH:mm:ss'));
        cell_state_td_63.html(cell_state_col_63);
        cell_state_tr_6.append([cell_state_td_61, cell_state_td_62, cell_state_td_63]);

        var cell_state_tr_7 = $('<tr></tr>').addClass('cell_state_tr');
        var cell_state_td_71 = $('<td></td>').addClass('cell_state_td').attr({
            colspan: 3
        }).css({
            'text-align': 'center'
        });
        var cell_state_col_71 = $('<input></input>').addClass('cell_state_col').attr({
            id: 'btn_update_cell',
            type: 'button'
        }).click(function () {
            $.ajax({
                url: 'http://localhost:32000/km/warehouse/cell/set/{0}/{1}/{2}'.Format([thisCell.Row, thisCell.Bay, thisCell.Level]),
                data: {
                    CellData: {
                        Row: thisCell.Row,
                        Bay: thisCell.Bay,
                        Level: thisCell.Level,
                        CellType: cell_state_col_02.val(),
                        CellStatus: cell_state_col_12.val(),
                        PalletID: cell_state_col_22.val(),
                        Prohibit: cell_state_col_32.prop('checked'),
                        SystemProhibit: cell_state_col_42.prop('checked'),
                        UpdateTime: new Date().Format('yyyy/MM/dd HH:mm:ss'),
                        StoreDeadline: '{0} {1}'.Format([cell_state_col_62.val(), cell_state_col_63.val()])
                    }
                },
                cache: false,
                type: 'POST',
                dataType: 'json',
                success: function (e) {
                    var resultDialog = $('<div></div>').addClass('ajax_result_dialog').attr({
                        title: '儲格更新結果'
                    });
                    var result = $('<span></span>').addClass('ajax_result').text(e.Status);

                    cell_state.append(resultDialog.html(result));
                    resultDialog.dialog();
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }).val('更新');
        cell_state_td_71.html(cell_state_col_71);
        cell_state_tr_7.append([cell_state_td_71]);
        //
        cell_state_frame.append([
            cell_state_title,
            cell_state_tr_0, cell_state_tr_1, cell_state_tr_2,
            cell_state_tr_3, cell_state_tr_4, cell_state_tr_5,
            cell_state_tr_6, cell_state_tr_7
        ]);
        cell_state.append([cell_state_frame]);
        $('body').append(cell_state);
        //
        cell_state.css({
            height: $(document).height() - $('.navbar').height()
        });
        cell_state_frame.css({
            top: ($(document).height() - cell_state_frame.height()) / 3,
            left: ($(document).width() - cell_state_frame.width()) / 2
        });
    },
    /**儲格種類 */
    CELL_TYPE: {
        'NAN': { text: '無', value: 0, isCostum: false },
        'QN': { text: '代驗區', value: 1, isCostum: false },
        'PL': { text: '棧板區', value: 2, isCostum: false },
        'QY': { text: '良品區', value: 3, isCostum: false },
        'CA': { text: '冷區', value: 4, isCostum: false },
        'PA': { text: '備料區', value: 5, isCostum: false }
        //{ text: '自訂理貨區', value: 6 }
    },
    /**儲格種類 選單 
     *@param currentValue {number}
     */
    CellTypeDropDownList: function (currentValue) {
        var selector = $('<select></select>').addClass('wh_cell_type_slt');
        var frame = $(document.createDocumentFragment());

        $.each(this.CELL_TYPE, function (index, item) {
            var item = $('<option></option>').addClass('wh_cell_type_item').attr({
                value: index
            }).text(item.text);

            frame.append(item);
        });

        selector.html(frame).val(currentValue);
        return selector;
    },
    /**
     * 取得客製 儲格種類
     * */
    GetCostumCellType: function () {
        $.ajax({
            url: '/asrs/costum/get/celltype',
            type: 'POST',
            dataType: 'json',
            cache: false,
            success: function (e) {
                if (e != undefined) {
                    $.each(e, function (index, item) {
                        ASRS_Warehouse.CELL_TYPE[item.CellTypeId] = new Object();
                        ASRS_Warehouse.CELL_TYPE[item.CellTypeId] = {
                            text: item.CellTypeId,
                            value: index + 6,
                            color: item.CellTypeColor,
                            isCostum: true
                        };
                    });
                }
            },
            error: function (e) {
                console.log(e)
            }
        });
    },
    /**儲格狀態 */
    CELL_STATUS: [
        { text: '空庫位', value: 0 },
        { text: '實庫位', value: 1 },
        { text: '預約入', value: 2 },
        { text: '預約出', value: 3 }
    ],
    /**儲格種類 選單 
     *@param currentValue {number}
     */
    CellStatusDropDownList: function (currentValue) {
        var selector = $('<select></select>').addClass('wh_cell_status_slt');
        var frame = $(document.createDocumentFragment());

        $.each(this.CELL_STATUS, function (index, item) {
            var item = $('<option></option>').addClass('wh_cell_status_item').attr({
                value: item.value
            }).text(item.text);

            frame.append(item);
        });

        selector.html(frame).val(currentValue);
        return selector;
    },
    /**事件 */
    Event: {
        OnCellStateChange: 'OnCellStateChange'
    }
}