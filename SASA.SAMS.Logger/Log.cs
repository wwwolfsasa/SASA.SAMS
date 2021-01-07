using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.ASRS.Logger {
    public class Log {
        /// <summary>
        /// 系統程式列表
        /// </summary>
        public enum App {
            /// <summary>
            /// 倉庫
            /// </summary>
            WAREHOUSE=0
        }

        /// <summary>
        /// Log 狀態
        /// </summary>
        public enum State {
            /// <summary>
            /// NLog Trace 
            /// </summary>
            TRACE = 0,
            /// <summary>
            /// NLog Info
            /// </summary>
            INFO = 1,
            /// <summary>
            /// NLog Error
            /// </summary>
            ERROR = 2,
            /// <summary>
            /// NLog Warn
            /// </summary>
            WARN = 3
        }

        /// <summary>
        /// 寫入 記錄檔
        /// </summary>
        /// <param name="application"></param>
        /// <param name="message"></param>
        public static void Write(State state, App application, string message) {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            switch (state) {
                case State s when s.Equals(State.TRACE):
                    logger.Trace(message);
                    break;
                case State s when s.Equals(State.INFO):
                    logger.Info(message);
                    break;
                case State s when s.Equals(State.ERROR):
                    logger.Error(message);
                    break;
                case State s when s.Equals(State.WARN):
                    logger.Warn(message);
                    break;
                default:
                    logger.Trace(message);
                    break;
            }
        }

    }
}
