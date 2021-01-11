$(function () {
    SAMS_PFD.InitialDeviceSetting();
});

var SAMS_PFD = {
    /**PFD 繪圖區 */
    PFD_CANVAS_CONTAINER: null,
    /**PFD 畫布*/
    PFD_CANVAS: null,
    /**元件初始化 */
    Initial: function () {
        this.PFD_CANVAS_CONTAINER = $('#pfd_tree');
    },
    /**畫布初始化 */
    InitialCanvas: function () {
        this.PFD_CANVAS_CONTAINER.height($(window).height() - $('.navbar').height());
        this.PFD_CANVAS = PFD_CANVAS_TOOL.CreateCanvas(this.PFD_CANVAS_CONTAINER.width(), this.PFD_CANVAS_CONTAINER.height());
        this.PFD_CANVAS.append(PFD_CANVAS_TOOL.ArrowDef());
        this.PFD_CANVAS_CONTAINER.html(this.PFD_CANVAS.prop('outerHTML'));
    },
    /**初始化裝置設定 */
    InitialDeviceSetting: function () {
        $('.device-darg').draggable({
            scroll: true,
            scrollSensitivity: 100,
            containment: '#pfd_tree',
            drag: function () {
                SAMS_PFD.ConnectDeviceDirection();
            },
            stop: function (e, device) {
                //console.log($(device.helper).attr('id'));
                //console.log(device);
                SAMS_PFD.ConnectDeviceDirection();
                //
                $.ajax({
                    url: 'http://localhost:32000/sams/device/set/position',
                    data: {
                        DeviceId: $(device.helper).attr('id'),
                        X: device.position.left,
                        Y: device.position.top
                    },
                    type: 'POST',
                    dataType: 'json',
                    cache: false,
                    success: function (e) {
                        if (!e.isSuccess) {
                            alert(e.Status);
                        }
                    },
                    error: function (e) {
                        console.log(e);
                    }
                });
            }
        });
        //
        this.ConnectDeviceDirection();
    },
    /**連接裝置物流方向 */
    ConnectDeviceDirection: function () {
        $('#pfd_tree svg').find('line').remove();
        $('svg>path').remove();
        //畫線
        var allDevices = $('.device-item');
        if (allDevices.length > 0) {
            $.each(allDevices, function () {
                //找到連線對象
                var conn = $(this).find('.device-conn[dir="OUT"], .device-conn[dir="INOUT"] ');
                if (conn.length > 0) {
                    var offset = $('#pfd_tree').offset();
                    var fromPointX = $(this).offset().left + $(this).outerWidth(true) / 2;
                    var fromPointY = $(this).offset().top + $(this).outerHeight(true) / 2;

                    $.each(conn, function (s,e) {
                        var toPoint = SAMS_PFD.CalcConnectPoint(fromPointX, fromPointY, $('.device-item[id="{0}"]'.Format([$(this).attr('conn')])));
                        //console.log(e)

                        var line = PFD_CANVAS_TOOL.CreateDraw('line', {
                            x1: (fromPointX - offset.left),
                            y1: (fromPointY - offset.top),
                            x2: (toPoint.x - offset.left),
                            y2: (toPoint.y - offset.top),
                            stroke: '#000000',
                            'marker-end': 'url(#arrow)'
                        });

                        $('#pfd_tree svg').append(line);
                    });
                }

            });
        }
    },
    /**
     * 計算連接點
     * @param {any} fromX
     * @param {any} fromY
     * @param {any} toPoint
     */
    CalcConnectPoint: function (fromX, fromY, toPoint) {
        var tx1 = fromX;
        var ty1 = fromY;
        var tx2 = toPoint.offset().left;
        var ty2 = toPoint.offset().top;

        var x2, y2, connType;
        switch (true) {
            case (tx1 == tx2 && ty1 > ty2):
            case (tx1 > tx2 && ty1 > ty2 && (ty1 - ty2) / (tx1 - tx2) > 1):
                x2 = tx2 + toPoint.outerWidth(true) / 2;
                y2 = ty2 + toPoint.outerHeight(true) + 5;
                connType = 0;
                break;
            case (tx1 < tx2 && ty1 > ty2 && (ty1 - ty2) / (tx1 - tx2) < -1):
                x2 = tx2 + toPoint.outerWidth(true) / 2;
                y2 = ty2 + toPoint.outerHeight(true) + 5;
                connType = 1;
                break;//
            case (tx1 < tx2 && ty1 == ty2):
            case (tx1 < tx2 && ty1 > ty2 && (ty1 - ty2) / (tx1 - tx2) >= -1 && (ty1 - ty2) / (tx1 - tx2) < 0):
                x2 = tx2 - 5;
                y2 = ty2 + toPoint.outerHeight(true) / 2;
                connType = 2;
                break;
            case (tx1 < tx2 && ty1 < ty2 && (ty1 - ty2) / (tx1 - tx2) <= 1 && (ty1 - ty2) / (tx1 - tx2) > 0):
                x2 = tx2 - 5;
                y2 = ty2 + toPoint.outerHeight(true) / 2;
                connType = 3;
                break;//
            case (tx1 == tx2 && ty1 < ty2):
            case (tx1 < tx2 && ty1 < ty2 && (ty1 - ty2) / (tx1 - tx2) > 1):
                x2 = tx2 + toPoint.outerWidth(true) / 2;
                y2 = ty2 - 5;
                connType = 4;
                break;
            case (tx1 > tx2 && ty1 < ty2 && (ty1 - ty2) / (tx1 - tx2) < -1):
                x2 = tx2 + toPoint.outerWidth(true) / 2;
                y2 = ty2 - 5;
                connType = 5;
                break;//
            case (tx1 > tx2 && ty1 == ty2):
            case (tx1 > tx2 && ty1 < ty2 && (ty1 - ty2) / (tx1 - tx2) >= -1 && (ty1 - ty2) / (tx1 - tx2) < 0):
                x2 = tx2 + toPoint.outerWidth(true) + 5;
                y2 = ty2 + toPoint.outerHeight(true) / 2;
                connType = 6;
                break;
            case (tx1 > tx2 && ty1 > ty2 && (ty1 - ty2) / (tx1 - tx2) <= 1 && (ty1 - ty2) / (tx1 - tx2) > 0):
                x2 = tx2 + toPoint.outerWidth(true) + 5;
                y2 = ty2 + toPoint.outerHeight(true) / 2;
                connType = 7;
                break;//
        }

        return {
            x: x2,
            y: y2,
            ConnectType: connType
        };
    }
}

var PFD_CANVAS_TOOL = {
    /**
     * 創建畫布
     * @param {number} width
     * @param {number} height
     */
    CreateCanvas: function (width, height) {
        var canvas = new $('<svg></svg>').attr({
            'width': width,
            'height': height,
            'version': 1.1,
            'xmlns': 'http://www.w3.org/2000/svg',
            'xmlns:xlink': 'http://www.w3.org/1999/xlink'
        });

        return canvas;
    },
    /**箭頭 物件 */
    ArrowDef: function () {
        var def = $('<defs></defs>');

        var arrow = this.CreateDraw('marker', {
            'id': 'arrow',
            'markerWidth': '12',
            'markerHeight': '12',
            'refX': '6',
            'refY': '6',
            'orient': 'auto'
        });

        var arrowPath = this.CreateDraw('path', {
            xmlns: 'http://www.w3.org/2000/svg',
            d: 'M2,2 L10,6 L2,10Z',
            fill: '#000000'
        });

        def.append(arrow.append(arrowPath));
        return def;
    },
    /**
     * 創建 SVG 元件
     * @param {string} tag
     * @param {any} attrs
     */
    CreateDraw: function (tag, attrs) {
        var draw = $(document.createElementNS('http://www.w3.org/2000/svg', tag));
        $.each(attrs, function (key, value) {
            draw.attr(key, value);
        });
        return draw;
    }
}