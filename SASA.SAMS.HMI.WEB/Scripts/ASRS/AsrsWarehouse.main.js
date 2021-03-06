﻿$(function () {
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
     * @param bay {number} 格
     * @param level {number} 層
     */
    OnCellClick: function (bay, level) {
        bay -= 1;
        level -= 1;
        var thisCell = this.SyncCellData[bay][level];

        $('.cell_state').remove();

        var cell_state = $('<div></div>').addClass('cell_state').click(function () {
            $('.ajax_result_dialog').remove();
            $('.pallet_info').remove();
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
        var cell_state_col_22 = this.PalletDropDownList(thisCell.PalletID);
        var cell_state_col_23 = $('<input></input>').addClass('cell_state_col').attr({
            id: 'btn_check_item_on_pallet',
            title: '棧板資訊',
            type: 'button'
        }).click(function () {
            cell_state.append(ASRS_PalletInfo.GetPalletInfo($(this), thisCell.PalletID));
        });
        var cell_state_col_24 = $('<input></input>').addClass('cell_state_col').attr({
            id: 'btn_new_pallet',
            title: '新增棧板',
            type: 'button'
        }).click(function () {
            var palletId = prompt('新增 棧板名稱');
            ASRS_Warehouse.NewPallet(palletId);
        });
        cell_state_td_22.append([cell_state_col_22, cell_state_col_23, cell_state_col_24]);
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
                    //reload
                    ASRS_Warehouse.GetCellState();
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
    /**
     * 新增 棧板
     * @param {string} palletId 棧板 ID
     */
    NewPallet: function (palletId) {
        if (palletId === '') {
            alert('棧板 ID 不可為空');
        } else {
            $.ajax({
                url: 'http://localhost:32000/km/pallet/new',
                data: {
                    PalletData: {
                        PalletID: palletId,
                        Items: null
                    }
                },
                type: 'POST',
                dataType: 'json',
                cache: false,
                success: function (e) {
                    console.log(e.Status);
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }
    },
    /**
     * 取得棧板清單
     * @param {any} currentPallet 當前儲位棧板
     */
    PalletDropDownList: function (currentPallet) {
        var selector = $('<select></select>').addClass('wh_cell_set_pallet').bind(this.Event.OnUnstorePalletListLoad, function (e, data) {
            $(this).html(data).val(currentPallet);
        });

        $.ajax({
            url: 'http://localhost:32000/km/pallet/get/unstore',
            type: 'POST',
            dataType: 'json',
            cache: false,
            success: function (e) {
                if (e.isSuccess) {
                    var frame = $(document.createDocumentFragment());

                    var nan = $('<option></option>').addClass('unstore_pallet').attr({
                        value: ''
                    }).text('無');
                    frame.append(nan);

                    if (currentPallet != '') {
                        var pallet_in_cell = $('<option></option>').addClass('unstore_pallet pallet_in_cell').attr({
                            value: currentPallet
                        }).text(currentPallet);

                        frame.append(pallet_in_cell);
                    }

                    $.each(e.Data, function (index, item) {
                        var item = $('<option></option>').addClass('unstore_pallet').attr({
                            value: item.PalletID
                        }).text(item.PalletID);

                        frame.append(item);
                    });

                    $('.wh_cell_set_pallet').trigger(ASRS_Warehouse.Event.OnUnstorePalletListLoad, [frame]);
                }

            },
            error: function (e) {
                console.log(e);
            }
        });

        return selector;
    },
    /**事件 */
    Event: {
        OnCellStateChange: 'OnCellStateChange',
        OnUnstorePalletListLoad: 'OnUnstorePalletListLoad'
    }
}

/**棧板資訊 */
var ASRS_PalletInfo = {
    /**取得 棧板上資訊
      * @param {string} palletId 棧板 ID
      * */
    GetPalletInfo: function (sender, palletId) {
        if (palletId === undefined) {
            alert('無棧板');
            return;
        }
        if (palletId === '') {
            alert('無棧板');
            return;
        }

        $('.pallet_info').remove();

        var pallet_info = $('<div></div>').addClass('pallet_info').css({
            top: sender.offset().top,
            left: sender.offset().left + sender.width() + 30
        }).click(function (e) {
            e.stopPropagation();
        });

        var pallet_info_table = $('<table></table>').addClass('pallet_info_table').attr({
            border: 1
        });

        var pallet_info_thead = $('<thead></thead>').addClass('pallet_info_thead');
        var pallet_info_thead_row = $('<tr></tr>');
        pallet_info_thead.html(pallet_info_thead_row);

        var pallet_info_thead_th_0 = $('<th></th>').addClass('pallet_info_thead_th').text('物件名稱');
        var pallet_info_thead_th_1 = $('<th></th>').addClass('pallet_info_thead_th').text('規格');
        var pallet_info_thead_th_2 = $('<th></th>').addClass('pallet_info_thead_th').text('描述');
        var pallet_info_thead_th_3 = $('<th></th>').addClass('pallet_info_thead_th').text('存量');
        pallet_info_thead_row.append([pallet_info_thead_th_0, pallet_info_thead_th_1, pallet_info_thead_th_2, pallet_info_thead_th_3]);

        var pallet_info_tbody = $('<tbody></tbody>').addClass('pallet_info_tbody').bind(this.Event.OnPalletInfoLoad, function (e, data) {
            $(this).find('.pallet_info_item').remove();
            //
            var bodyFrag = $(document.createDocumentFragment());

            $.each(data.Items, function (index, item) {
                var row = $('<tr></tr>').addClass('pallet_info_item');

                var c_0 = $('<td></td>').addClass('pallet_info_item_data');
                var set_0 = $('<input></input>').addClass('pallet_info_set_data').attr({
                    type: 'text',
                    name: 'ItemName'
                }).val(item.ItemName);
                c_0.html(set_0);

                var c_1 = $('<td></td>').addClass('pallet_info_item_data');
                var set_1 = $('<input></input>').addClass('pallet_info_set_data').attr({
                    type: 'text',
                    name: 'Spec'
                }).val(item.Spec);
                c_1.html(set_1);

                var c_2 = $('<td></td>').addClass('pallet_info_item_data');
                var set_2 = $('<input></input>').addClass('pallet_info_set_data').attr({
                    type: 'text',
                    name: 'Description'
                }).val(item.Description);
                c_2.html(set_2);

                var c_3 = $('<td></td>').addClass('pallet_info_item_data');
                var set_3 = $('<input></input>').addClass('pallet_info_set_data').attr({
                    type: 'text',
                    name: 'Accumulation'
                }).val(item.Accumulation);
                c_3.html(set_3);

                bodyFrag.append(row.append([c_0, c_1, c_2, c_3]));
            });
            //
            $(this).html(bodyFrag);
        });
        pallet_info_table.append([pallet_info_thead, pallet_info_tbody]);

        var pallet_info_tool_bar = $('<div></div>').addClass('pallet_info_tool_bar');
        var pallet_info_tool_item_0 = $('<input></input>').addClass('pallet_info_tool_item').attr({
            type: 'button'
        }).click(function () {
            ASRS_PalletInfo.SavePalletItems(palletId);
            //
            $(this).hide();
            $('.new_pallet_item').remove();
        }).val('保存').hide();
        var pallet_info_tool_item_1 = $('<input></input>').addClass('pallet_info_tool_item').attr({
            type: 'button'
        }).click(function () {
            pallet_info_tool_item_0.show();
            pallet_info_tbody.append(ASRS_PalletInfo.NewItemRow());
        }).val('新增');
        var pallet_info_tool_item_2 = $('<input></input>').addClass('pallet_info_tool_item').attr({
            type: 'button'
        }).click(function () {

        }).val('清空');
        pallet_info_tool_bar.append([pallet_info_tool_item_0, pallet_info_tool_item_1, pallet_info_tool_item_2]);

        //first load pallet items
        this.LoadPalletItems(palletId);
        //
        return pallet_info.append([pallet_info_tool_bar, pallet_info_table]);
    },
    /**新增棧板上品項 */
    NewItemRow: function () {
        var row = $('<tr></tr>').addClass('pallet_info_item new_pallet_item');

        var c_0 = $('<td></td>').addClass('pallet_info_item_data');
        var set_0 = $('<input></input>').addClass('pallet_info_set_data').attr({
            type: 'text',
            name: 'ItemName'
        });
        c_0.html(set_0);

        var c_1 = $('<td></td>').addClass('pallet_info_item_data');
        var set_1 = $('<input></input>').addClass('pallet_info_set_data').attr({
            type: 'text',
            name: 'Spec'
        });
        c_1.html(set_1);

        var c_2 = $('<td></td>').addClass('pallet_info_item_data');
        var set_2 = $('<input></input>').addClass('pallet_info_set_data').attr({
            type: 'text',
            name: 'Description'
        });
        c_2.html(set_2);

        var c_3 = $('<td></td>').addClass('pallet_info_item_data');
        var set_3 = $('<input></input>').addClass('pallet_info_set_data').attr({
            type: 'text',
            name: 'Accumulation'
        });
        c_3.html(set_3);

        return row.append([c_0, c_1, c_2, c_3]);
    },
    /**載入棧板資訊
     * @param {string} palletId 棧板 ID
     * */
    LoadPalletItems: function (palletId) {
        $.ajax({
            url: 'http://localhost:32000/km/pallet/get/info/{0}'.Format([palletId]),
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (e) {
                if (e.isSuccess) {
                    $('.pallet_info_tbody').trigger(ASRS_PalletInfo.Event.OnPalletInfoLoad, [e.Data]);
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    },
    /**保存棧板資訊
     * @param {string} palletId 棧板 ID
     * */
    SavePalletItems: function (palletId) {
        var rows = $('.pallet_info_item');
        var items = new Array();

        $.each(rows, function (sender) {
            var itemName = $(this).find('.pallet_info_set_data[name="ItemName"]').val();
            var spec = $(this).find('.pallet_info_set_data[name="Spec"]').val();
            var description = $(this).find('.pallet_info_set_data[name="Description"]').val();
            var acc = $.isNumeric($(this).find('.pallet_info_set_data[name="Accumulation"]').val()) ? parseInt($(this).find('.pallet_info_set_data[name="Accumulation"]').val()) : 0;

            items.push({
                ItemName: itemName,
                Spec: spec,
                Description: description,
                Accumulation: acc
            });
        });

        $.ajax({
            url: 'http://localhost:32000/km/pallet/modify/info',
            data: {
                PalletId: palletId,
                PalletItems: items
            },
            type: 'POST',
            dataType: 'json',
            cache: false,
            success: function (e) {
                if (e.isSuccess) {
                    ASRS_PalletInfo.LoadPalletItems(palletId);
                } else {
                    alert(e.Status);
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    },
    /**事件 */
    Event: {
        OnPalletInfoLoad: 'OnPalletInfoLoad'
    }
}

/**棧板資訊 */
class PalletInfo {
    /**初始化
     * @param {string} ItemName 物件名稱
     * @param {string} Spec 規格
     * @param {string} Description 描述
     * @param {number} Accumulation 存量
     * */
    constructor(ItemName, Spec, Description, Accumulation) {
        /**物件名稱
         * @type {string}
         * */
        this.ItemName = ItemName;
        /**規格
         * @type {string}
         * */
        this.Spec = Spec;
        /**描述
         * @type {string}
         * */
        this.Description = Description;
        /**存量
         * @type {number}
         * */
        this.Accumulation = Accumulation;
    }
}