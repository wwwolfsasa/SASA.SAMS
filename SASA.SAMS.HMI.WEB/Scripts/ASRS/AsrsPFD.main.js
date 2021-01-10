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
        this.PFD_CANVAS_CONTAINER.html(this.PFD_CANVAS);
    },
    /**初始化裝置設定 */
    InitialDeviceSetting: function () {
        $('.device-darg').draggable({
            scroll: true,
            scrollSensitivity: 100,
            containment: '#pfd_tree',
            stop: function (e, device) {
                //console.log($(device.helper).attr('id'));
                //console.log(device);

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
        })
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
            'viewBox': '0 0 {0} {1}'.Format([width, height]),
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