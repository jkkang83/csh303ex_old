using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static MotorizedStage_SK_PI.F_Motion_SK_PI;

namespace MotorizedStage_SK_PI
{
    public partial class F_Motion_SK_PI : Form
    {
        // Controller
        private static readonly SK_Motion SK = new SK_Motion();
        private static readonly PI_Motion PI = new PI_Motion();
        // Axis
        private readonly Axis[] _skAxes;
        private readonly Axis[] _piAxes;
        private readonly int _cntSKAxis;
        private readonly int _cntPIAxis;
        private readonly int _cntAxis;
        private readonly int _cntMaxAxis = 6;

        private string _csFileName = "MotionCS.txt";
        private string _speedFileName = "MotionSpeed.txt";
        private readonly string _homeFileName = "MotionHome.txt";
        private double[] _homePositions;

        public enum SpeedLevel { Slow, Normal, Fast }
        private double[][] _speedLevelValues;

        // Asix Move UI List

        private List<RadioButton> _rdoAxisList;
        private List<RadioButton> _rdoSpeedLevelMList;

        // Axis Set UI List
        private List<RadioButton> _rdoSpeedLevelSList;
        private List<TextBox> _txtSpeedLevelList;
        private List<TextBox> _txtHomeList;
        private List<TextBox> _txtPivotList;
        private List<TextBox> _txtCSList;

        // Asix Status UI List 
        private List<TextBox> _txtPositionList;
        private List<TextBox> _txtSpeedList;
        private List<Label> _lblIsBusyList;
        private List<Label> _lblIsReadyList;
        private List<Label> _lblAlarmList;
        private List<Label> _lblIsPosLimitList;
        private List<Label> _lblIsNegLimitList;
        private List<Label> _lblIsHomeList;
        

        #region 프로퍼티
        private Axis? CurrentAxis
        {
            get
            {
                //if (InvokeRequired)
                //{
                //    return (Axis?)Invoke(new Func<Axis?>(() =>
                //    {
                //        for (int iAxis = 0; iAxis < _rdoAxisList.Count; iAxis++)
                //        {
                //            if (_rdoAxisList[iAxis].Checked)
                //            {
                //                return (Axis)iAxis;
                //            }
                //        }
                //        return null;
                //    }));

                //}
                //else
                //{
                    for (int iAxis = 0; iAxis < _rdoAxisList.Count; iAxis++)
                    {
                        if (_rdoAxisList[iAxis].Checked)
                        {
                            return (Axis)iAxis;
                        }
                    }
                    return null;
                //}
            }
        }
       
        private SpeedLevel? SpeedLevelSetGrp
        {
            get
            {
                for (int level = 0; level < _rdoSpeedLevelSList.Count; level++)
                {
                    if (_rdoSpeedLevelSList[level].Checked)
                    {
                        return (SpeedLevel)level;
                    }
                }
                return null;
            }
        }
        
        private SpeedLevel? SpeedLevelMovGrp
        {
            get
            {
                for (int level = 0; level < _rdoSpeedLevelMList.Count; level++)
                {
                    if (_rdoSpeedLevelMList[level].Checked)
                    {
                        return (SpeedLevel)level;
                    }
                }
                return null;

            }
        }

        private bool IsConnected
        {
            get
            {
                if (_cntPIAxis > 0 && _cntSKAxis > 0)
                {
                    // SK, PI 둘다 연결됐는지 확인
                    if (SK.IsConnected && PI.IsConnected)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (SK.IsConnected || PI.IsConnected)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        #endregion

        public F_Motion_SK_PI(string homeFileName = null, string CSFileName = null, string SpeedFileName = null)
        {
            this.CreateHandle();
            InitializeComponent();
            // 축의 컨트롤로 변경시 여기서 수정
            _skAxes = new Axis[] { Axis.X, Axis.Y, Axis.Z, };
            _piAxes = new Axis[] { Axis.TX, Axis.TY, Axis.TZ };
            if (_skAxes != null)
            {
                _cntSKAxis = _skAxes.Length;
            }
            if (_piAxes != null)
            {
                _cntPIAxis = _piAxes.Length;
            }
            _cntAxis = _cntSKAxis + _cntPIAxis;

            _speedLevelValues = new double[_cntMaxAxis][];
            _homePositions = new double[_cntMaxAxis];

            // Asix Move UI List
            _rdoAxisList = new List<RadioButton>
            {
                XAxisRdo, YAxisRdo, ZAxisRdo,
                TXAxisRdo, TYAxisRdo, TZAxisRdo
            };
            _rdoSpeedLevelMList = new List<RadioButton>
            {
                SlowMRdo, NormalMRdo, FastMRdo
            };

            // Asix Set UI List
            _rdoSpeedLevelSList = new List<RadioButton>
            {
                SlowSRdo, NormalSRdo, FastSRdo
            };
            _txtSpeedLevelList = new List<TextBox>
            {
                XSpeedLevelTxt, YSpeedLevelTxt, ZSpeedLevelTxt,
                TXSpeedLevelTxt, TYSpeedLevelTxt, TZSpeedLevelTxt
            };
            _txtHomeList = new List<TextBox>
            {
                XHomeTxt, YHomeTxt, ZHomeTxt,
                TXHomeTxt, TYHomeTxt, TZHomeTxt,
            };
            _txtPivotList = new List<TextBox>
            {
                XPivotTxt, YPivotTxt, ZPivotTxt
            };
            _txtCSList = new List<TextBox>
            {
                XCSTxt, YCSTxt, ZCSTxt,
                TXCSTxt, TYCSTxt, TZCSTxt
            };

            // 축 상태 UI List
            _txtPositionList = new List<TextBox>
            {
                XCurPosTxt, YCurPosTxt, ZCurPosTxt,
                TXCurPosTxt, TYCurPosTxt, TZCurPosTxt
            };
            _txtSpeedList = new List<TextBox>
            {
                XSpeedTxt, YSpeedTxt, ZSpeedTxt,
                TXSpeedTxt, TYSpeedTxt, TZSpeedTxt
            };
            _lblIsBusyList = new List<Label>
            {
                XIsBusyLbl, YIsBusyLbl, ZIsBusyLbl,
                TXIsBusyLbl, TYIsBusyLbl, TZIsBusyLbl
            };
            _lblIsReadyList = new List<Label>
            {
                XIsReadyLbl, YIsReadyLbl, ZIsReadyLbl,
                TXIsReadyLbl, TYIsReadyLbl, TZIsHomeLbl
            };
            _lblAlarmList = new List<Label>
            {
                XAlarmLbl, YAlarmLbl, ZAlarmLbl,
                TXAlarmLbl, TYAlarmLbl, TZAlarmLbl
            };
            _lblIsPosLimitList = new List<Label>
            {
                XIsPosLimitLbl, YIsPosLimitLbl, ZIsPosLimitLbl,
                TXIsPosLimitLbl, TYIsPosLimitLbl, TZIsPosLimitLbl
            };
            _lblIsNegLimitList = new List<Label>
            {
                XIsNegLimitTxt, YIsNegLimitTxt, ZIsNegLimitTxt,
                TXIsNegLimitTxt, TYIsNegLimitTxt, TZIsNegLimitTxt
            };
            _lblIsHomeList = new List<Label>
            {
               XIsHomeLbl, YIsHomeLbl, ZIsHomeLbl,
               TXIsHomeLbl, TYIsHomeLbl, TZIsHomeLbl
            };

            PivotGrp.Enabled = false;   // 안씀

            if (!string.IsNullOrEmpty(homeFileName))
            {
                _homeFileName = homeFileName;
            }

            if (!string.IsNullOrEmpty(CSFileName))
            {
                _csFileName = CSFileName;
            }
            if (!string.IsNullOrEmpty(SpeedFileName))
            {
                _speedFileName = SpeedFileName;
            }
            
            // ui컨트롤 Enabled 설정 // 초기 
            UpdateUIIsConnect(false);

            SK.OnConnectionChange += Motion_Connected;
            SK.OnStatusChanged += Motion_StatusChanged;
            SK.Logged += SK_Motion_Logged;

            PI.OnConnectionChange += Motion_Connected;
            PI.OnStatusChanged += Motion_StatusChanged;
            PI.Logged += PI_Motion_Logged;
        }

        #region public 함수(델리게이트 연결 가능)
        // Move
        public void MoveAbsAxis(Axis axis, double targetPos)
        {
            Task.Run(() => MoveAbsAxisAsync(axis, targetPos)).GetAwaiter().GetResult();
        }

        public async Task MoveAbsAxisAsync(Axis axis, double targetPos)
        {
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                await SK.MoveAsync(new Axis[1] { axis }, new double[1] { targetPos }, true);
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                await PI.MoveAsync(new Axis[1] { axis }, new double[1] { targetPos }, true);
            }
        }
        
        public void MoveAbs6D(double x, double y, double z, double tx, double ty, double tz)
        {
            Task.Run(() => MoveAbs6DAsync(x, y, z, tx, ty, tz)).GetAwaiter().GetResult();
        }
        public async Task MoveAbs6DAsync(double x, double y, double z, double tx, double ty, double tz)
        {
            double[] targetPos = new double[6] {x, y, z, tx, ty, tz};
            double[] _skPositions = new double[_cntSKAxis];
            double[] _piPositions = new double[_cntPIAxis];
            //SK

            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            if (_skAxes != null)
            {
                for (int i = 0; i < _cntSKAxis; i++)
                {
                    _skPositions[i] = targetPos[(int)_skAxes[i]];
                }

                skTask = SK.MoveAsync(_skAxes, _skPositions, true);
            }

            Task piTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            if (_piAxes != null)
            {
                //PI
                for (int i = 0; i < _cntPIAxis; i++)
                {
                    _piPositions[i] = targetPos[(int)_piAxes[i]];
                }
                piTask = PI.MoveAsync(_piAxes, _piPositions, true);
            }

            await Task.WhenAll(skTask, piTask);
        }
        public void MoveSkXYZ(double x, double y, double z)
        {
            Task.Run(() => MoveAbsSkXYZAsync(x, y, z)).GetAwaiter().GetResult();
        }

        public async Task MoveAbsSkXYZAsync(double x, double y, double z)
        {
            double[] targetPos = new double[3] { x, y, z };
            double[] _skPositions = new double[_cntSKAxis];
            //SK

            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            if (_skAxes != null)
            {
                for (int i = 0; i < _cntSKAxis; i++)
                {
                    _skPositions[i] = targetPos[(int)_skAxes[i]];
                }

                skTask = SK.MoveAsync(_skAxes, _skPositions, true);
            }

            await Task.WhenAll(skTask);
        }

        public void MoveHexapodRotation(double tx, double ty, double tz)
        {
            Task.Run(() => MoveHexapodRotationAsync( tx, ty, tz)).GetAwaiter().GetResult();
        }
        public async Task MoveHexapodRotationAsync(double tx, double ty, double tz)
        {
            double[] targetPos = new double[6] { 0,0,0,tx, ty, tz };
            double[] _piPositions = new double[_cntPIAxis];

            Task piTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            if (_piAxes != null)
            {
                //PI
                for (int i = 0; i < _cntPIAxis; i++)
                {
                    _piPositions[i] = targetPos[(int)_piAxes[i]];
                }
                piTask = PI.MoveAsync(_piAxes, _piPositions, true);
            }

            await Task.WhenAll(piTask);
        }


        // Hexapod x축 움직이기 위해 임치 추가
        //public void MoveHexapodAxis(Axis axis, double targetPos)
        //{
        //    Task.Run(() => MoveHexapodAxisAsync(axis, targetPos)).GetAwaiter().GetResult();
        //}
        //public async Task MoveHexapodAxisAsync(Axis axis, double targetPos)
        //{
        //    if (_piAxes != null)
        //    {
        //        await PI.MoveAsync(new Axis[1] { axis }, new double[1] { targetPos }, true);
        //    }
        //}

        public void MoveRelAxis(Axis axis, double targetPos)
        {
            Task.Run(() => MoveRelAxisAsync(axis, targetPos)).GetAwaiter().GetResult();
        }

        public async Task MoveRelAxisAsync(Axis axis, double targetPos)
        {
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                await SK.MoveAsync(new Axis[1] { axis }, new double[1] { targetPos }, false);
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                await PI.MoveAsync(new Axis[1] { axis }, new double[1] { targetPos }, false);
            }
        }

        public void MoveRel6D(double x, double y, double z, double tx, double ty, double tz)
        {
            Task.Run(() => MoveRel6DAsync(x,y,z,tx,ty,tz)).GetAwaiter().GetResult();
        }

        public async Task MoveRel6DAsync(double x, double y, double z, double tx, double ty, double tz)
        {
            double[] targetPos = new double[6] { x, y, z, tx, ty, tz };
            double[] _skPositions = new double[_cntSKAxis];
            double[] _piPositions = new double[_cntPIAxis];
            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            Task piTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정

            // SK
            if (_skAxes != null)
            {
                for (int i = 0; i < _cntSKAxis; i++)
                {
                    _skPositions[i] = targetPos[(int)_skAxes[i]];
                }
                skTask =  SK.MoveAsync(_skAxes, _skPositions, false);
            }

            // PI
            if (_piAxes != null)
            {
                for (int i = 0; i < _cntPIAxis; i++)
                {
                    _piPositions[i] = targetPos[(int)_piAxes[i]];
                }
                piTask = PI.MoveAsync(_piAxes, _piPositions, false);
            }

            await Task.WhenAll(skTask, piTask);
        }

        public void ReferenceAxis(Axis axis)
        {
            Task.Run(() => ReferenceAxisAsync(axis)).GetAwaiter().GetResult();
        }

        public async Task ReferenceAxisAsync(Axis axis)
        {
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                await SK.ReferenceAsync(new Axis[] { axis }, new bool[] { true });
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                await PI.ReferenceAsync();
            }
        }

        public void Reference6D()
        {
            Task.Run(() => Reference6DAsync()).GetAwaiter().GetResult();
        }

        public async Task Reference6DAsync()
        {
            bool[]  sk_origin = Enumerable.Repeat(true, _cntSKAxis).ToArray();
            bool[] pi_origin = Enumerable.Repeat(true, _cntPIAxis).ToArray();
            Task skTask = Task.CompletedTask; 
            Task piTask = Task.CompletedTask; 

            if (_skAxes != null)
            {
                skTask = SK.ReferenceAsync(_skAxes, sk_origin);
            }

            if (_piAxes != null)
            {
                // PI 추가
                piTask = PI.ReferenceAsync();
            }

            await Task.WhenAll(skTask, piTask);
        }

        public async Task ReferenceSKAsync()
        {
            bool[] sk_origin = Enumerable.Repeat(true, _cntSKAxis).ToArray();

            if (_skAxes != null)
            {
                await SK.ReferenceAsync(_skAxes, sk_origin);
            }
        }

        public async Task ReferencePIAsync()
        {
            if (_piAxes != null)
            {
                await PI.ReferenceAsync();
            }
        }

        public void MoveHomeAxis(Axis axis)
        {
            Task.Run(() => MoveHomeAxisAsync(axis)).GetAwaiter().GetResult();
        }

        public async Task MoveHomeAxisAsync(Axis axis)
        {
            int iAxis = (int)axis;
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                await SK.MoveAsync(new Axis[1] { axis }, new double[1] { _homePositions[iAxis] }, true);
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                await PI.MoveAsync(new Axis[1] { axis }, new double[1] { _homePositions[iAxis] }, true);
            }
        }
        
        public void MoveHome6D()
        {
            Task.Run(() => MoveHome6DAsync()).GetAwaiter().GetResult();
        }

        public async Task MoveHome6DAsync()
        {
            double[] skHomePositions = new double[_cntSKAxis];
            double[] piHomePositions = new double[_cntPIAxis];
            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            Task piTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정

            if (_skAxes != null)
            {
                for (int i = 0; i < _cntSKAxis; i++)
                {
                    skHomePositions[i] = _homePositions[(int)_skAxes[i]];
                }

                skTask = SK.MoveAsync(_skAxes, skHomePositions, true);
            }

            if (_piAxes != null)
            {
                for (int i = 0; i < _cntPIAxis; i++)
                {
                    piHomePositions[i] = _homePositions[(int)_piAxes[i]];
                }
                // 저장된 값 으로 이동
                piTask = PI.MoveAsync(_piAxes, piHomePositions, true);

            }

            await Task.WhenAll(skTask, piTask);
        }
        
        public void MoveOriginAxis(Axis axis)
        {
            Task.Run(() => MoveOriginAxisAsync(axis)).GetAwaiter().GetResult();
        }

        public async Task MoveOriginAxisAsync(Axis axis)
        {
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                await SK.MoveAsync(new Axis[1] { axis }, new double[1] {0}, true);
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                await PI.MoveAsync(new Axis[1] { axis }, new double[1] {0}, true);
            }
        }

        public void MoveOrigin6D()
        {
            Task.Run(() => MoveOrigin6DAsync()).GetAwaiter().GetResult();
        }

        public async Task MoveOrigin6DAsync()
        {
            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            Task piTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정

            if (_skAxes != null)
            {
                skTask = SK.MoveAsync(_skAxes, new double[_cntSKAxis], true);
            }

            if (_piAxes != null)
            {
                piTask = PI.MoveAsync(_piAxes, new double[_cntPIAxis], true);
            }

            await Task.WhenAll(skTask, piTask);
        }

        public void MoveOriginHexapod()
        {
            Task.Run(() => MoveOriginHexapodAsynce()).GetAwaiter().GetResult();
        }

        public async Task MoveOriginHexapodAsynce()
        {
            if (_piAxes != null)
            {
                // await PI.MoveAsync(new Axis[] { Axis.X, Axis.Y, Axis.Z, Axis.TX, Axis.TY, Axis.TZ }, new double[6], true);
                await PI.MoveAsync(_piAxes, new double[_cntPIAxis], true);
            }
        }

        // Jog
        public void JogRun(Axis axis, bool dir, SpeedLevel speedLevel)
        {
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                SetSpeedAxis(axis, speedLevel);
                SK.JogRun(axis, dir);
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                SetSpeedAxis(axis, speedLevel);
                PI.JogRun(axis, dir);
            }
        }

        public void JogStop(Axis axis)
        {
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                SK.JogStop(axis);
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                PI.JogStop(axis);
            }
        }
        
        // Stop
        public async Task StopAsync(Axis axis)
        {
            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            Task piTask = Task.CompletedTask; 

            if (_skAxes != null && _skAxes.Contains(axis))
            {
                skTask = SK.StopAsync(new Axis[1] { axis }, new bool[1] { true });
            }
            else if (_piAxes != null && _piAxes.Contains (axis))
            {
                piTask = PI.StopAsync(new Axis[1] { axis }, new bool[1] { true });
            }

            await Task.WhenAll(skTask, piTask);
        }

        private async Task StopEmergencyAsync()
        {
            Task skTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정
            Task piTask = Task.CompletedTask; // 기본값으로 완료된 Task 설정

            if (_skAxes != null)
            {
                skTask= SK.StopEmergencyAsync();
            }
            if (_piAxes != null)
            {
                piTask= PI.StopEmergencyAsync();
            }
            await Task.WhenAll(skTask, piTask);
        }

        // Set
        public void SetHome6DFormCurPos()
        {
            double[] Positions = GetCurPos6D();
            _homePositions = Positions.Select(pos => pos).ToArray();
            // tx,ty,tz는 0으로 저장
            _homePositions[3] = _homePositions[4] = _homePositions[5] = 0;
            SaveHomeFile();
            UpdateUISetHome();
        }
        public void SetHome6D(double[] homes)
        {
            _homePositions = homes.Select(home => home).ToArray();
            SaveHomeFile();
            UpdateUISetHome();
        }

        public void SetSpeedLevelValue(double[] speedLevelValues, SpeedLevel level)
        {
            for(int axis = 0; axis < _cntMaxAxis; axis++ )
            {
                if (_speedLevelValues[axis] == null)
                {
                    _speedLevelValues[axis] = new double[3];
                }

                if (_skAxes != null && _skAxes.Contains((Axis)axis))
                {
                    _speedLevelValues[axis][(int)level] = speedLevelValues[axis];
                }
                else if (_piAxes != null && Array.IndexOf(_piAxes, (Axis)axis) == (_cntPIAxis-1))
                {
                    foreach (Axis piAxis in _piAxes)
                    {
                        int iPiAxis = (int)piAxis;
                        _speedLevelValues[iPiAxis][(int)level] = speedLevelValues[axis];
                    }
                }
            }

            UpdateUISetSpeedLevel();

            // Speed 현재 Move에 선택되있는 speedLevel 콤보버튼, 축 콤보버튼에 해당하는 speedLevel로 설정
            if (this.InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if(SpeedLevelMovGrp != level) { return; }

                    if (AllAxesRdo.Checked)
                    {
                        SetSpeed6D(level);
                    }
                    else
                    {
                        SetSpeedAxis((Axis)CurrentAxis, level);
                    }
                }));
            }
            else
            {
                if (SpeedLevelMovGrp != level) { return; }

                if (AllAxesRdo.Checked)
                {
                    SetSpeed6D(level);
                }
                else
                {
                    SetSpeedAxis((Axis)CurrentAxis, level);
                }
            }
        }
        public void SetSpeed6D(SpeedLevel speedLevel)
        {
            int iSpeedLevel = (int)speedLevel;

            // 모든 축
            foreach (Axis axis in Enum.GetValues(typeof(Axis)))
            {
                int iAxis = (int)axis;
                double speedValue = _speedLevelValues[iAxis][iSpeedLevel];
                if (_skAxes != null && _skAxes.Contains(axis))
                {
                    SK.SetSpeed(axis, (int)speedValue / 10, (int)speedValue, 45);  // 초기 100% 300, 60% 180, 50% 150, 25% 75 15% 45
                }
            }
            if (_piAxes != null && _cntPIAxis > 0)
            {
                double speedValue = _speedLevelValues[(int)_piAxes[0]][iSpeedLevel];
                PI.SetSpeed(speedValue*1.5);
            }

            UpdateUISetSpeed();
        }
        public void SetSpeedAxis(Axis axis, SpeedLevel speedLevel)
        {
            int iSpeedLevel = (int)speedLevel;
            int iAxis = (int)axis;
            double speedValue = _speedLevelValues[iAxis][iSpeedLevel];
            // 개별 축
            if (_skAxes != null && _skAxes.Contains(axis))
            {
                SK.SetSpeed(axis, (int)speedValue / 10, (int)speedValue, 45);  // 초기 300, 60% 180, 50% 150, 25% 75, 15% 45
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                PI.SetSpeed(speedValue*1.5);
            }
            UpdateUISetSpeed();
        }
        public void SetPivot(double x, double y, double z)
        {
            if (!PI.IsConnected) { return; }
            SetCoordinateSystem(x, y, z, 0, 0, 0);

            //PI.SetPivot(x, y, z);
            //UpdateUISetPivot();
        }
        public void SetCoordinateSystem(double u, double v, double w)
        {
            if (!PI.IsConnected) { return; }
            double[] pos = PI.GetCoordinateSystem();
            SetCoordinateSystem(pos[0], pos[1], pos[2], u, v, w);
        }
        public void SetCoordinateSystem(double x, double y, double z, double u, double v, double w)
        {
            if (!PI.IsConnected) { return; }
            PI.SetCoordinateSystem(x, y, z, u, v, w);
            SaveCSFile(new double[] { x, y, z, u, v, w });
            UpdateUISetCoordinateSystem();
        }

        // Get
        public double[] GetPivot()
        {
            if (!PI.IsConnected) { return null; }
            return PI.GetPivot();
        }
        public double[] GetCoordinateSystem()
        {
            if (!PI.IsConnected) { return null; }
            return PI.GetCoordinateSystem();
        }
        public double GetCurPosAxis(Axis axis)
        {
            double position = 0;

            if (_skAxes != null && _skAxes.Contains(axis))
            {
                int index = Array.IndexOf(_skAxes, axis);
                position = SK.GetPositions()[index];
            }
            else if (_piAxes != null && _piAxes.Contains(axis))
            {
                int index = Array.IndexOf(_piAxes, axis);
                // pi
                position = 0;
            }
            return position;
        }
        public double[] GetCurPos6D()
        {
            double[] positions = new double[_cntAxis];
            double[] sk_Positions = SK.GetPositions();
            double[] pi_Positions = PI.GetPositions();

            foreach (Axis axis in Enum.GetValues(typeof(Axis)))
            {
                int iAxis = (int)axis;
                if (_skAxes != null && _skAxes.Contains(axis))
                {
                    int index = Array.IndexOf(_skAxes, axis);
                    positions[iAxis] = sk_Positions[index];
                }
                else if ( _piAxes != null && _piAxes.Contains(axis))
                {
                    int index = Array.IndexOf(_piAxes, axis);
                    positions[iAxis] = pi_Positions[index];
                }
            }
            return positions;
        }
        public double[] GetCurPosHexapod()
        {
            if (!PI.IsConnected) { return null; }

            return PI.GetAllPositions();
        }

        public bool ConnectSKPI()
        {
            bool res = false;
            if (SK.IsConnected || PI.IsConnected)
            {
                SK.Disconnect();
                PI.Disconnect();
            }
            else
            {
                if (_cntSKAxis != 0)
                {
                    string port = ComPortCmb.SelectedItem?.ToString();

                    if (string.IsNullOrEmpty(port))
                    {
                        port = "COM10";
                    }

                    res = SK.Connect(_skAxes, port);
                }
                if (_cntPIAxis != 0)
                {
                    PI.Connect(_piAxes, "169.254.62.155");
                }
            }
            return res;
        }
        #endregion

        #region private 함수
        private double[] LoadHomeFile()
        {
            try
            {
                string filePath = _homeFileName;
                double[] positions = new double[_cntMaxAxis];

                if (!File.Exists(filePath))
                {
                    SaveHomeFile(positions);
                }
                else
                {
                    StreamReader rd = new StreamReader(filePath);
                    string lstr = rd.ReadToEnd();
                    rd.Close();

                    string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    positions = allLines.Select(pos => double.Parse(pos)).ToArray();
                }
                
                homeFileTxt.Text = _homeFileName;

                return positions;
            }
            catch
            {
                AppendLogMessage("Failed to load Logical home position");
                return null;
            }
        }
        private void SaveHomeFile(double[] positions = null)
        {
            try
            {
                string filePath = _homeFileName;

                if (positions == null)
                {
                    positions = _homePositions;
                }

                StreamWriter wr = new StreamWriter(filePath);
                for (int i = 0; i < _cntAxis; i++)
                {
                    wr.WriteLine(positions[i].ToString());
                }
                wr.Close();
            }
            catch
            {
                AppendLogMessage("Failed to save Logical home position");
            }

        }

        private double[] LoadCSFile()
        {
            try
            {
                string filePath = _csFileName;
                double[] positions = new double[_cntMaxAxis];

                if (!File.Exists(filePath))
                {
                    SaveCSFile(positions);
                }
                else
                {
                    StreamReader rd = new StreamReader(filePath);
                    string lstr = rd.ReadToEnd();
                    rd.Close();

                    string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    positions = allLines.Select(pos => double.Parse(pos)).ToArray();
                }

                //homeFileTxt.Text = _csFileName;

                return positions;
            }
            catch
            {
                AppendLogMessage("Failed to load Logical home position");
                return null;
            }
        }
        private void SaveCSFile(double[] positions = null)
        {
            try
            {
                string filePath = _csFileName;

                if (positions == null)
                {
                    positions = PI.GetCoordinateSystem();
                }

                StreamWriter wr = new StreamWriter(filePath);
                for (int i = 0; i < _cntAxis; i++)
                {
                    wr.WriteLine(positions[i].ToString());
                }
                wr.Close();
            }
            catch
            {
                AppendLogMessage("Failed to save Logical home position");
            }

        }

        private double[] LoadSpeedFile()
        {
            try
            {
                string filePath = _speedFileName;
                double[] positions = new double[_cntMaxAxis];

                if (!File.Exists(filePath))
                {
                    //SaveSpeedFile(positions);
                }
                else
                {
                    StreamReader rd = new StreamReader(filePath);
                    string lstr = rd.ReadToEnd();
                    rd.Close();

                    string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    positions = allLines.Select(pos => double.Parse(pos)).ToArray();
                }

                //homeFileTxt.Text = _csFileName;

                return positions;
            }
            catch
            {
                AppendLogMessage("Failed to load Logical home position");
                return null;
            }
        }
        private void SaveSpeedFile()
        {
            try
            {
                string filePath = _csFileName;


                StreamWriter wr = new StreamWriter(filePath);
                foreach (Axis axis in Enum.GetValues(typeof(Axis)))
                {
                    int iAxis = (int)axis;
                    if (_skAxes != null && _skAxes.Contains(axis))
                    {
                        //wr.WriteLine(SK.DefaultSpeedLevelValues[iAxis][iSpeedLevel].ToString());
                    }
                    else if (_piAxes != null && _piAxes.Contains(axis))
                    {
                       // wr.WriteLine(PI.DefaultSpeedLevelValues[iSpeedLevel].ToString());
                    }
                }
                wr.Close();
            }
            catch
            {
                AppendLogMessage("Failed to save Logical home position");
            }

        }

        // UI Connected / Disconned Control 설정
        private void UpdateUIIsConnect(bool isEnabled)
        {
            if(this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    MoveGrp.Enabled = isEnabled;
                    SetGrp.Enabled = isEnabled;

                    MoveTypeCbo.Items.Clear();
                    MoveTypeCbo.Items.AddRange(new String[] { "Absolute", "Relative" });
                    MoveTypeCbo.SelectedIndex = 0;


                    if (isEnabled)
                    {
                        ConnectBtn.Text = "Disconnect";
                        IsCunnectedLbl.BackColor = Color.Blue;

                        _rdoAxisList[0].Select();
                        IsEditChk.Checked = false;
                        NormalMRdo.Checked = true;
                        NormalSRdo.Checked = true;
                    }
                    else
                    {
                        ConnectBtn.Text = "Connect";
                        IsCunnectedLbl.BackColor = Color.Silver;
                    }
                }));
            }
            else
            {
                MoveGrp.Enabled = isEnabled;
                SetGrp.Enabled = isEnabled;

                MoveTypeCbo.Items.Clear();
                MoveTypeCbo.Items.AddRange(new String[] { "Absolute", "Relative" });
                MoveTypeCbo.SelectedIndex = 0;


                if (isEnabled)
                {
                    ConnectBtn.Text = "Disconnect";
                    IsCunnectedLbl.BackColor = Color.Blue;

                    _rdoAxisList[0].Select();
                    //NormalMRdo.Checked = true;
                    //NormalSRdo.Checked = true;
                    IsEditChk.Checked = false;
                }
                else
                {
                    ConnectBtn.Text = "Connect";
                    IsCunnectedLbl.BackColor = Color.Silver;
                }
            }
        }
       
        private void UpdateUIAxisStatus(AxisStatus[] axisStatuses)
        {
            if (axisStatuses == null || axisStatuses.Length > _cntAxis) { return; }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < axisStatuses.Length; i++)
                    {
                        AxisStatus status = axisStatuses[i];
                        int iAxis = (int)status.Axis;

                        _txtPositionList[iAxis].Text = status.Position.ToString();
                        _lblIsBusyList[iAxis].BackColor = status.IsMoving ? Color.Red : Color.LightGray;
                        _lblIsReadyList[iAxis].BackColor = status.IsMoving ? Color.LightGray : Color.Lime;

                        var color = Color.LightGray;
                        switch (status.Alarm)
                        {
                            case 40:
                            case 20:
                                color = Color.Red;
                                break;
                            case 10:
                                color = Color.Orange;
                                break;
                            case 0:
                                color = Color.Lime;
                                break;
                            default:
                                break;
                        }

                        _lblAlarmList[iAxis].BackColor = color;
                        _lblIsPosLimitList[iAxis].BackColor = status.IsPosLimit ? Color.Red : Color.LightGray;
                        _lblIsNegLimitList[iAxis].BackColor = status.IsNegLimit ? Color.Red : Color.LightGray;
                        _lblIsHomeList[iAxis].BackColor = (status.Position == _homePositions[iAxis]) ? Color.Lime : Color.LightGray;  // 현재위치랑 홈이랑 비교해서 추가해야함.
                    }
                }));
            }
            else
            {
                for (int i = 0; i < axisStatuses.Length; i++)
                {
                    AxisStatus status = axisStatuses[i];
                    int iAxis = (int)status.Axis;

                    _txtPositionList[iAxis].Text = status.Position.ToString();
                    _lblIsBusyList[iAxis].BackColor = status.IsMoving ? Color.Red : Color.LightGray;
                    _lblIsReadyList[iAxis].BackColor = status.IsMoving ? Color.LightGray : Color.Lime;

                    var color = Color.LightGray;
                    switch (status.Alarm)
                    {
                        case 40:
                        case 20:
                            color = Color.Red;
                            break;
                        case 10:
                            color = Color.Orange;
                            break;
                        case 0:
                            color = Color.Lime;
                            break;
                        default:
                            break;
                    }

                    _lblAlarmList[iAxis].BackColor = color;
                    _lblIsPosLimitList[iAxis].BackColor = status.IsPosLimit ? Color.Red : Color.LightGray;
                    _lblIsNegLimitList[iAxis].BackColor = status.IsNegLimit ? Color.Red : Color.LightGray;
                    _lblIsHomeList[iAxis].BackColor = (status.Position == _homePositions[iAxis]) ? Color.Lime : Color.LightGray;  // 현재위치랑 홈이랑 비교해서 추가해야함.
                }
            }
        }

        // UI Axis Set Update
        private void UpdateUISetHome()
        {
            
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    for (int axis = 0; axis < _cntAxis; axis++)
                    {
                        // 설정된 Home 
                        _txtHomeList[axis].Text = _homePositions[axis].ToString();
                    }
                }));
            }
            else
            {
                for (int axis = 0; axis < _cntAxis; axis++)
                {
                    // 설정된 Home 
                    _txtHomeList[axis].Text = _homePositions[axis].ToString();
                }
            }
        }
        private void UpdateUISetSpeedLevel()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    for (int axis = 0; axis < _cntAxis; axis++)
                    {
                        // 설정된 각 SpeedLevel의 Value
                        _txtSpeedLevelList[axis].Text = _speedLevelValues[axis][(int)SpeedLevelSetGrp].ToString();
                    }
                }));
            }
            else
            {
                for (int axis = 0; axis < _cntAxis; axis++)
                {
                    // 설정된 각 SpeedLevel의 Value
                    _txtSpeedLevelList[axis].Text = _speedLevelValues[axis][(int)SpeedLevelSetGrp].ToString();
                }
            }
        }
        private void UpdateUISetSpeed()
        {
            double[] skSpeeds = null;
            double[] piSpeeds = null;

            if (_cntSKAxis > 0)
            {
                skSpeeds = SK.GetSpeeds();
            }
            if (_cntPIAxis > 0)
            {
                piSpeeds = PI.GetSpeeds();
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (Axis axis in Enum.GetValues(typeof(Axis)))
                    { 
                        // 설정된 각 SpeedLevel의 Value
                        int iAxis = (int)axis;
                        if (_skAxes != null && _skAxes.Contains(axis))
                        {
                            int index = Array.IndexOf(_skAxes, axis);
                            _txtSpeedList[iAxis].Text = skSpeeds[index].ToString();
                        }
                        else if (_piAxes != null && _piAxes.Contains(axis))
                        {
                            int index = Array.IndexOf(_piAxes, axis);
                            _txtSpeedList[iAxis].Text = piSpeeds[index].ToString();
                        }
                    }
                }));
            }
            else
            {
                foreach (Axis axis in Enum.GetValues(typeof(Axis)))
                {
                    // 설정된 각 SpeedLevel의 Value
                    int iAxis = (int)axis;
                    if (_skAxes != null && _skAxes.Contains(axis))
                    {
                        int index = Array.IndexOf(_skAxes, axis);
                        _txtSpeedList[iAxis].Text = skSpeeds[index].ToString();
                    }
                    else if (_piAxes != null && _piAxes.Contains(axis))
                    {
                        int index = Array.IndexOf(_piAxes, axis);
                        _txtSpeedList[iAxis].Text = piSpeeds[index].ToString();
                    }
                }
            }
        }
        private void UpdateUISetPivot()
        {
            double[] pivot = GetPivot();

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    _txtPivotList[0].Text = pivot[0].ToString();
                    _txtPivotList[1].Text = pivot[1].ToString();
                    _txtPivotList[2].Text = pivot[2].ToString();
                }));
            }
            else
            {
                _txtPivotList[0].Text = pivot[0].ToString();
                _txtPivotList[1].Text = pivot[1].ToString();
                _txtPivotList[2].Text = pivot[2].ToString();
            }
        }
        private void UpdateUISetCoordinateSystem()
        {
            double[] cs = GetCoordinateSystem();

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < cs.Length; i++)
                    {
                        _txtCSList[i].Text = cs[i].ToString();
                    }
                }));
            }
            else
            {
                for (int i = 0; i < cs.Length; i++)
                {
                    _txtCSList[i].Text = cs[i].ToString();
                }
            }
        }
        // UI 로그 메시지를 TextBox에 추가
        public string mInitialMsg = "";
        public void AppendLogMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() => LogText.AppendText(message + Environment.NewLine)));
            }
            else
            {
                LogText.AppendText(message + Environment.NewLine);
            }
            
        }
        
        #endregion

         
        #region Motion 이벤트
        private void Motion_Connected(object sender, bool isConnected)
        {
            if (IsConnected)
            {
                SetHome6D(LoadHomeFile());
                foreach (SpeedLevel speedLevel in Enum.GetValues(typeof(SpeedLevel)))
                {
                    var iSpeedLevel = (int)speedLevel;
                    var speedLevelValues = new double[_cntMaxAxis];
                    
                    foreach (Axis axis in Enum.GetValues(typeof(Axis)))
                    {
                        int iAxis = (int)axis;
                        if (_skAxes != null && _skAxes.Contains(axis))
                        {
                            speedLevelValues[iAxis] = SK.DefaultSpeedLevelValues[iAxis][iSpeedLevel];
                        }
                        else if (_piAxes != null && _piAxes.Contains(axis))
                        {
                            speedLevelValues[iAxis] = PI.DefaultSpeedLevelValues[iSpeedLevel];
                        }
                    }

                    SetSpeedLevelValue(speedLevelValues, speedLevel);
                }

                //SetPivot(0, 0, 0);

                var csPos = LoadCSFile();
                SetCoordinateSystem(csPos[0], csPos[1], csPos[2], csPos[3], csPos[4], csPos[5]);

                UpdateUIAxisStatus(SK.GetAxisStatuses());
                UpdateUIAxisStatus(PI.GetAxisStatuses());

                //double[] piPos = GetCurPosHexapod();
                //AppendLogMessage($"{piPos[0]},{piPos[1]},{piPos[2]},{piPos[3]},{piPos[4]},{piPos[5]}");
            }
            UpdateUIIsConnect(IsConnected);
        }
        
        private void Motion_StatusChanged(object sender, AxisStatus[] e)
        {
            if (IsConnected)
            {
                UpdateUIAxisStatus(e);
            }
        }
       
        private void SK_Motion_Logged(object sender, string e)
        {
            AppendLogMessage($"SK {e}");
        }
        private void PI_Motion_Logged(object sender, string e)
        {
            AppendLogMessage($"PI {e}");
        }
        #endregion

        #region UI 이벤트
        private void F_Motion_SK_PI_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 프로그램 종료할때
            if (SK.IsConnected)
            {
                SK.Disconnect();
            }
            if (PI.IsConnected)
            {
                PI.Disconnect();
            }

            SK.OnConnectionChange -= Motion_Connected;
            SK.OnStatusChanged -= Motion_StatusChanged;
            SK.Logged -= SK_Motion_Logged;

            PI.OnConnectionChange -= Motion_Connected;
            PI.OnStatusChanged -= Motion_StatusChanged;
            PI.Logged -= PI_Motion_Logged;
        }

        private void ComPortCmb_DropDown(object sender, EventArgs e)
        {
            string[] ports = SK.GetPortNames();
            ComPortCmb.Items.Clear();
            ComPortCmb.Items.AddRange(ports);
        }

        private void AxisRdo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdo = (RadioButton)sender;

            if (rdo == AllAxesRdo)
            {
                bool isVisible = AllAxesRdo.Checked;
                AllAxesMovPnl.Visible = isVisible;
                AxisMovPnl.Visible = !isVisible;
            }

            // 선택됐을때 이벤트만 받을 거임
            if (!rdo.Checked) { return; }

            if (SpeedLevelMovGrp == null) { return; }
            SpeedLevel speedLevel = (SpeedLevel)SpeedLevelMovGrp;
            
            if (AllAxesRdo.Checked)
            {
                SetSpeed6D(speedLevel);
            }
            else
            {
                SetSpeedAxis((Axis)CurrentAxis,speedLevel);
            }
        }
        private void SpeedLevelMRdo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            if (!btn.Checked) { return; }

            if (SpeedLevelMovGrp == null) { return; }
            SpeedLevel speedLevel = (SpeedLevel)SpeedLevelMovGrp;

            if (AllAxesRdo.Checked)
            {
                SetSpeed6D(speedLevel);
            }
            else
            {
                SetSpeedAxis((Axis)CurrentAxis, speedLevel);
            }
        }
        // Connect
        private void ConnectPortBtn_Click(object sender, EventArgs e)
        {
            ConnectSKPI();
        }

        

        // Jog
        private void JogMinusBtn_MouseDown(object sender, MouseEventArgs e)
        {
            if (CurrentAxis == null || SpeedLevelMovGrp == null) return;
            //await JogRunAsync((Axis)CurrentAxis, false, (SpeedLevel)SpeedLevelMovGrp);
            JogRun((Axis)CurrentAxis, false, (SpeedLevel)SpeedLevelMovGrp);

        }

        private void JogPlusBtn_MouseDown(object sender, MouseEventArgs e)
        {
            if (CurrentAxis == null || SpeedLevelMovGrp == null) return;

            //await JogRunAsync((Axis)CurrentAxis, true, (SpeedLevel)SpeedLevelMovGrp);
            JogRun((Axis)CurrentAxis, true, (SpeedLevel)SpeedLevelMovGrp);
        }

        private void JogBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (CurrentAxis == null) return;
            //await StopAsync((Axis)CurrentAxis);
            JogStop((Axis)CurrentAxis);
        }

        // Move
        private async void MoveAxisBtn_Click(object sender, EventArgs e)
        {
            if (MoveTypeCbo.SelectedIndex == -1) { return; }

            if (CurrentAxis == null) { return; }

            MoveAxisBtn.Enabled = false;
            Axis axis = (Axis)CurrentAxis;

            double position = double.Parse(targetPosTxt.Text);
            bool isAbsolute = MoveTypeCbo.SelectedIndex == 0 ? true : false;

            if (isAbsolute)
            {
                await MoveAbsAxisAsync(axis, position);
            }
            else
            {
                await MoveRelAxisAsync(axis, position);
            }

            MoveAxisBtn.Enabled = true;

        }

        private async void StopAxisBtn_Click(object sender, EventArgs e)
        {
            if (CurrentAxis == null) return;

            StopAxisBtn.Enabled = false;

            await StopAsync((Axis)CurrentAxis);

            StopAxisBtn.Enabled = true;
        }

        private async void Move6DBtn_Click(object sender, EventArgs e)
        {
            if (MoveTypeCbo.SelectedIndex == -1) { return; }
            if (!AllAxesRdo.Checked) { return; }

            Move6DBtn.Enabled = false;

            List<TextBox> txtTargetPosList = new List<TextBox>
            { 
                XtargetPosTxt, YtargetPosTxt, ZtargetPosTxt,
                TXtargetPosTxt, TYtargetPosTxt, TZtargetPosTxt
            };

            double[] positions = new double[_cntAxis];
            for (int i = 0; i < _cntAxis; i++)
            {
                if (!double.TryParse(txtTargetPosList[i].Text, out positions[i]))
                {
                    AppendLogMessage($"Invalid input for axis {i}: {txtTargetPosList[i].Text}");
                    return;
                }
            }

            bool isAbsolute = MoveTypeCbo.SelectedIndex == 0 ? true : false;

            if (isAbsolute)
            {
                await MoveAbs6DAsync(positions[0], positions[1], positions[2], positions[3], positions[4], positions[5]);
            }
            else
            {
                await MoveRel6DAsync(positions[0], positions[1], positions[2], positions[3], positions[4], positions[5]);
            }

            Move6DBtn.Enabled = true;
        }

        private async void Stop6DBtn_Click(object sender, EventArgs e)
        {
            if (!AllAxesRdo.Checked) { return; }

            Stop6DBtn.Enabled = false;

            await StopEmergencyAsync();

            Stop6DBtn.Enabled = true;
        }

        // Home
        private async void MechOriginBtn_Click(object sender, EventArgs e)
        {
            MechOriginBtn.Enabled = false;

            if (AllAxesRdo.Checked)
            {
                await MoveOrigin6DAsync();
            }
            else
            {
                Axis axis = (Axis)CurrentAxis;
                await MoveOriginAxisAsync(axis);
            }

            MechOriginBtn.Enabled = true;
        }

        private async void VisionHomeBtn_Click(object sender, EventArgs e)
        {
            if (SpeedLevelMovGrp == null) { return; }

            VisionHomeBtn.Enabled = false;

            if (AllAxesRdo.Checked)
            {
                await MoveHome6DAsync();
            }
            else
            {
                Axis axis = (Axis)CurrentAxis;
                await MoveHomeAxisAsync(axis);
            }
        
            VisionHomeBtn.Enabled = true; 
        }

        private async void StopBtn_Click(object sender, EventArgs e)
        {
            StopBtn.Enabled = false;

            await StopEmergencyAsync();

            StopBtn.Enabled = true;
        }

        // Set
        private void IsEditChk_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (IsEditChk.Checked)
                {
                    for (int axis = 0; axis < _cntAxis; axis++)
                    {
                        _txtSpeedLevelList[axis].ReadOnly = false;
                        _txtHomeList[axis].ReadOnly = false;
                        _txtCSList[axis].ReadOnly = false;
                    }
                }
                else
                {
                    
                    UpdateUISetSpeedLevel();
                    UpdateUISetHome();
                    // UpdateUISetPivot();
                    UpdateUISetCoordinateSystem();

                    // TextBox 설정
                    for (int axis = 0; axis < _cntAxis; axis++)
                    {
                        _txtSpeedLevelList[axis].ReadOnly = true;
                        _txtHomeList[axis].ReadOnly = true;
                        _txtCSList[axis].ReadOnly = true;
                    }

                }
            }
            catch (Exception ex) { AppendLogMessage(ex.Message); }
            
        }
        
        private void CurPosToHomeBtn_Click(object sender, EventArgs e)
        {
            if (!IsEditChk.Checked) { return; }

            double[] CurPos = GetCurPos6D();
            for (int i = 0; i < _cntAxis; i++)
            {
                _txtHomeList[i].Text = CurPos[i].ToString();
            }
        }

        private void SetBtn_Click(object sender, EventArgs e)
        {
            if (!IsEditChk.Checked) { return; }

            try
            {
                var speedLevelValue = new double[_cntMaxAxis];
                var homePostions = new double[_cntMaxAxis];
                var pivot = new double[3];
                var cs = new double[_cntMaxAxis];
                for (int axis = 0; axis < _cntMaxAxis; axis++)
                {
                    // Speed
                    speedLevelValue[axis] = double.Parse(_txtSpeedLevelList[axis].Text);

                    // Home
                    homePostions[axis] = double.Parse(_txtHomeList[axis].Text);

                    // pivot
                    if (axis < (int)Axis.TX)
                    {
                        pivot[axis] = double.Parse(_txtPivotList[axis].Text);
                    }

                    // CoordinateSystem
                    cs[axis] = double.Parse(_txtCSList[axis].Text);
                };

                // SpeedLevel
                SetSpeedLevelValue(speedLevelValue, (SpeedLevel)SpeedLevelSetGrp);
               
                // Home
                SetHome6D(homePostions);

                // Pivot
                //SetPivot(pivot[0], pivot[1], pivot[2]);

                // CoordinateSystem
                SetCoordinateSystem(cs[0], cs[1], cs[2], cs[3], cs[4], cs[5]);
            }
            catch (Exception ex) { AppendLogMessage(ex.Message); }
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            if (!IsEditChk.Checked) { return; }

            try
            {
                // Speed Level Values
                var iSpeedLevel = (int)SpeedLevelSetGrp;
                var speedLevelValues = new double[_cntMaxAxis];
                foreach (Axis axis in Enum.GetValues(typeof(Axis)))
                {
                    int iAxis = (int)axis;
                    if (_skAxes != null && _skAxes.Contains(axis))
                    {
                        speedLevelValues[iAxis] = SK.DefaultSpeedLevelValues[iAxis][iSpeedLevel];
                    }
                    else if (_piAxes != null && _piAxes.Contains(axis))
                    {
                        speedLevelValues[iAxis] = PI.DefaultSpeedLevelValues[iSpeedLevel];
                    }
                }
                SetSpeedLevelValue(speedLevelValues, (SpeedLevel)SpeedLevelSetGrp);

                // Home
                SetHome6D(new double[_cntMaxAxis]);    // 모두 0으로.
                // CoordinateSystem
                SetCoordinateSystem(0, 0, 0, 0, 0, 0);

                //SetPivot(0,0,0);



            }
            catch (Exception ex) { AppendLogMessage(ex.Message); }
        }

        private void SpeedLevelSRdo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            if (btn.Checked)
            {
                UpdateUISetSpeedLevel();
            }
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void LogClearBtn_Click(object sender, EventArgs e)
        {
            LogText.Text= "";
        }

        private async void ReferencXYZBtn_Click(object sender, EventArgs e)
        {
            ReferencXYZBtn.Enabled = false;

            await ReferenceSKAsync();

            ReferencXYZBtn.Enabled = true;
        }

        private async void ReferenceHexapodBtn_Click(object sender, EventArgs e)
        {
            ReferenceHexapodBtn.Enabled = false;

            await ReferencePIAsync();

            ReferenceHexapodBtn.Enabled = true;
        }

        #endregion

        private void F_Motion_SK_PI_Load(object sender, EventArgs e)
        {
            AppendLogMessage(mInitialMsg);
        }


        bool isTesting = false;
        bool stopCommand = false;

        private async void btnXYZTest_Click(object sender, EventArgs e)
        {
            string xyzPosFile = "C:\\CSHTest\\DoNotTouch\\Admin\\XYZPos.csv";

            if (!isTesting)
            {
                isTesting = true;
                btnXYZTest.Text = "Stop";

                SetSpeed6D(SpeedLevel.Normal);
                await Task.Run(async () =>
                {
                    while (!stopCommand)
                    {
                        for (int idxAxis = 0; idxAxis < _cntSKAxis; idxAxis++)
                        {
                            
                            MoveHome6D();
                            await Task.Delay(500);
                            Axis axis = (Axis)idxAxis;
                            if (stopCommand) break;
                            MoveRelAxis(axis, -2000);
                            await Task.Delay(500);

                            double[] xyzPos = GetCurPos6D();
                            File.AppendAllText(xyzPosFile, $"{DateTime.Now:MM/dd/yyyy H:mm},{axis},{xyzPos[0]:F3},{xyzPos[1]:F3},{xyzPos[2]:F3}\r\n");

                            if (stopCommand) break;
                            MoveRelAxis(axis, 4000);
                            
                            xyzPos = GetCurPos6D();
                            File.AppendAllText(xyzPosFile, $"{DateTime.Now:MM/dd/yyyy H:mm},{axis},{xyzPos[0]:F3},{xyzPos[1]:F3},{xyzPos[2]:F3}\r\n");
                        }
                    }
                });

                isTesting = false;
                stopCommand = false;
                btnXYZTest.Text = "XYZ Move";
            }
            else
            {
                stopCommand = true;
            }

        }
    }
}