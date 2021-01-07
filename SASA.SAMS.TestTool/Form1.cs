using KM.ASRS.Warehouse;
using KM.ASRS.Warehouse.Manager;
using KM.MITSU.DE.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KM.ASRS.TestTool {
    public partial class Form1: Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            wc?.StopServer();
        }


        WarehouseController wc = null;
        private void button1_Click(object sender, EventArgs e) {
             wc = new WarehouseController();
            wc.OpenWarehouse();
            var cell = wc.GetCell(2,3,4);

            wc.CloseWarehouse();
        }

        private void button2_Click(object sender, EventArgs e) {
            wc = new WarehouseController();
            if (EthernetPort.InsureTcpPort(wc.ServerPort) && EthernetPort.InsureFirewall("Kenmec-Warehouse", this.wc.ServerPort)) {
                wc.StartServer();
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            var en = ShelfHelper.GetEnumByIndex<ShelfHelper.CELL_STATUS>(2);
        }
    }
}
