﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;

namespace ZSMeasure
{
    public class HalconHelp
    {
        HTuple hv_DispWindown;
        HTuple hv_MinAreaMark = null, hv_MaxAreaMark = null;
        HTuple hv_MinAreaPoint = null, hv_MaxAreaPoint = null;
        HTuple hv_CCDMarkY = null, hv_CCDMarkX = null, hv_CCDAxisY = null, hv_CCDAxisX = null;
        HTuple hv_HomMat2D = null;
        HTuple hv_Sx = null, hv_Sy = null, hv_Phi = null, hv_Theta = null;
        HTuple hv_Tx = null, hv_Ty = null;


        /// <summary>
        /// 显示窗体句柄
        /// </summary>
        /// <param name="Window"></param>
        public void SetDispWindow(HTuple Window)
        {
            hv_DispWindown = Window;
        }

        /// <summary>
        /// 设置Mark点面积范围
        /// </summary>
        /// <param name="MarkArea"></param>
        public void SetMarkArea(int[] MarkArea)
        {
            hv_MinAreaMark = MarkArea[0];
            hv_MaxAreaMark = MarkArea[1];
        }

        /// <summary>
        /// 设置九点面积范围
        /// </summary>
        /// <param name="MarkArea"></param>
        public void SetPointArea(int[] PointArea)
        {
            hv_MinAreaPoint = PointArea[0];
            hv_MaxAreaPoint = PointArea[1];
        }

        /// <summary>
        /// 仿射矩阵
        /// </summary>
        /// <param name="path"></param>
        /// <param name="camname"></param>
        /// <returns></returns>
        public bool SetHomMat2D(string path, string camname)
        {
            try
            {
                HOperatorSet.ReadTuple(path + "\\" + camname + "MarkY.tup", out hv_CCDMarkY);
                int countMY = hv_CCDMarkY.Length;
                HOperatorSet.ReadTuple(path + "\\" + camname + "MarkX.tup", out hv_CCDMarkX);
                int countMX = hv_CCDMarkX.Length;
                HOperatorSet.ReadTuple(path + "\\" + camname + "AxisY.tup", out hv_CCDAxisY);
                int countAY = hv_CCDAxisY.Length;
                HOperatorSet.ReadTuple(path + "\\" + camname + "AxisX.tup", out hv_CCDAxisX);
                int countAX = hv_CCDMarkX.Length;
                if (countMY < 9 || countMX < 9 || countAY < 9 || countAX < 9 || countAX != countAY || countAX != countMX || countMX != countMY)
                {
                    return false;
                }
                HOperatorSet.VectorToHomMat2d(hv_CCDMarkX, hv_CCDMarkY, hv_CCDAxisX, hv_CCDAxisY, out hv_HomMat2D);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// 获得点的实际坐标
        /// </summary>
        /// <param name="markCenter"></param>
        /// <returns></returns>
        public PointH GetMarkAxis(PointH markCenter)
        {
            HTuple hv_Tx = null, hv_Ty = null;
            try
            {
                HOperatorSet.AffineTransPoint2d(hv_HomMat2D, markCenter.X, markCenter.Y, out hv_Tx, out hv_Ty);
                return new PointH(hv_Tx[0].D, hv_Ty[0].D);
            }
            catch { return new PointH(); }
        }

        // Local procedures 
        public PointH GetMarkCenter(HObject ho_Image)
        {
            // Local iconic variables 
            HObject ho_Regions, ho_Regions1, ho_RegionClosing, ho_RegionOpening;
            HObject ho_ConnectedRegions, ho_SelectedRegions2, ho_RegionFillUp1, ho_RegionFillUp2;
            HObject ho_SelectedRegions1;

            // Local control variables 
            HTuple hv_AreaMax = null;
            HTuple hv_Y, hv_X;
            HTuple hv_Width, hv_Height, hv_Area;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Regions1);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);

            try
            {
                //Find by region
                ho_Regions.Dispose();
                HOperatorSet.AutoThreshold(ho_Image, out ho_Regions, 2);
                //ho_RegionClosing.Dispose();
                //HOperatorSet.ClosingCircle(ho_Regions, out ho_RegionClosing, 3.5);
                //ho_RegionOpening.Dispose();
                //HOperatorSet.OpeningCircle(ho_RegionClosing, out ho_RegionOpening, 3.5);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_ConnectedRegions, out ho_RegionFillUp1);
                ho_SelectedRegions2.Dispose();
                HOperatorSet.SelectShape(ho_RegionFillUp1, out ho_SelectedRegions2, "area", "and", hv_MinAreaMark, hv_MaxAreaMark);
                ho_RegionFillUp2.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions2, out ho_RegionFillUp2);
                
                ho_RegionOpening.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionFillUp2, out ho_RegionOpening, 3.5);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_RegionFillUp2, out ho_RegionOpening, 3.5);
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_RegionOpening, out ho_RegionFillUp1);

                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShapeStd(ho_RegionFillUp1, out ho_SelectedRegions1, "max_area", 70);
                HOperatorSet.AreaCenter(ho_SelectedRegions1, out hv_AreaMax, out hv_Y, out hv_X);
                HOperatorSet.RegionFeatures(ho_SelectedRegions1, "width", out hv_Width);
                HOperatorSet.RegionFeatures(ho_SelectedRegions1, "height", out hv_Height);
                HOperatorSet.RegionFeatures(ho_SelectedRegions1, "area", out hv_Area);
                HOperatorSet.SetColor(hv_DispWindown, "red");
                HalconHelp halconHelp = new HalconHelp();
                string strArea = "";
                for (int i = 0; i < hv_Area.Length; i++)
                {
                    strArea += hv_Area[i].D.ToString() + ",";
                }
                halconHelp.disp_message(hv_DispWindown, "Area:" + strArea.TrimEnd(','), "image", 10, 1000, "red", "true");
                if (hv_X.Length <= 0 || hv_Y.Length <= 0)
                {
                    //未找到Mark点
                    return new PointH();
                }
                double dd = hv_Width[0].D - hv_Height[0].D;
                if (Math.Abs(dd) > 3)
                {
                    //Mark点在视野的边界
                    return new PointH();
                }
                HOperatorSet.DispObj(ho_SelectedRegions1, hv_DispWindown);
                return new PointH(hv_X[0], hv_Y[0]);
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Regions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionFillUp2.Dispose();
                ho_SelectedRegions1.Dispose();

                throw HDevExpDefaultException;
            }
            finally
            {
                ho_Regions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionFillUp2.Dispose();
                ho_SelectedRegions1.Dispose();
            }
        }

        public PointH GetMarkCenterXLD(HObject ho_Image)
        {
            // Local iconic variables 
            HObject ho_Regions, ho_Regions1, ho_RegionClosing, ho_RegionOpening;
            HObject ho_ConnectedRegions, ho_SelectedRegions2, ho_RegionFillUp1, ho_RegionFillUp2;
            HObject ho_SelectedRegions1;
            HObject ho_ROIOuter, ho_ROIInner, ho_ROI, ho_ROIEdges, ho_RimReduced;
            HObject ho_Edges, ho_ContoursSplit, ho_RelEdges, ho_UnionContours;
            HObject ho_ContCircle1;
            HTuple hv_Number = null, hv_Row3 = null, hv_Column3 = null;
            HTuple hv_Radius2 = null, hv_StartPhi2 = null, hv_EndPhi2 = null;
            HTuple hv_PointOrder3 = null;

            // Local control variables 
            HTuple hv_AreaMax = null;
            HTuple hv_Y, hv_X;
            HTuple hv_Width, hv_Height, hv_Area;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Regions1);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);

            HOperatorSet.GenEmptyObj(out ho_ROIOuter);
            HOperatorSet.GenEmptyObj(out ho_ROIInner);
            HOperatorSet.GenEmptyObj(out ho_ROI);
            HOperatorSet.GenEmptyObj(out ho_ROIEdges);
            HOperatorSet.GenEmptyObj(out ho_RimReduced);
            HOperatorSet.GenEmptyObj(out ho_Edges);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplit);
            HOperatorSet.GenEmptyObj(out ho_RelEdges);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_ContCircle1);

            try
            {
                //Find by region
                ho_Regions.Dispose();
                HOperatorSet.AutoThreshold(ho_Image, out ho_Regions, 2);
                //ho_RegionClosing.Dispose();
                //HOperatorSet.ClosingCircle(ho_Regions, out ho_RegionClosing, 3.5);
                //ho_RegionOpening.Dispose();
                //HOperatorSet.OpeningCircle(ho_RegionClosing, out ho_RegionOpening, 3.5);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_ConnectedRegions, out ho_RegionFillUp1);
                ho_SelectedRegions2.Dispose();
                HOperatorSet.SelectShape(ho_RegionFillUp1, out ho_SelectedRegions2, "area", "and", hv_MinAreaMark, hv_MaxAreaMark);
                ho_RegionFillUp2.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions2, out ho_RegionFillUp2);

                ho_RegionOpening.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionFillUp2, out ho_RegionOpening, 3.5);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_RegionFillUp2, out ho_RegionOpening, 3.5);
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_RegionOpening, out ho_RegionFillUp1);

                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShapeStd(ho_RegionFillUp1, out ho_SelectedRegions1, "max_area", 70);
                HOperatorSet.AreaCenter(ho_SelectedRegions1, out hv_AreaMax, out hv_Y, out hv_X);
                HOperatorSet.RegionFeatures(ho_SelectedRegions1, "width", out hv_Width);
                HOperatorSet.RegionFeatures(ho_SelectedRegions1, "height", out hv_Height);
                HOperatorSet.RegionFeatures(ho_SelectedRegions1, "area", out hv_Area); 
                HalconHelp halconHelp = new HalconHelp();
                HOperatorSet.SetColor(hv_DispWindown, "red");
                string strArea = "";
                for (int i = 0; i < hv_Area.Length; i++)
                {
                    strArea += hv_Area[i].D.ToString() + ",";
                }
                halconHelp.disp_message(hv_DispWindown, "Area:" + strArea.TrimEnd(','), "image", 10, 1000, "red", "true");
                //HOperatorSet.SetColor(hv_DispWindown, "red");
                if (hv_X.Length <= 0 || hv_Y.Length <= 0)
                {
                    //未找到Mark点
                    return new PointH();
                }
                double dd = hv_Width[0].D - hv_Height[0].D;
                if (Math.Abs(dd) > 9)
                {
                    //Mark点在视野的边界
                    return new PointH();
                }


                //Find by XLD
                HOperatorSet.DilationCircle(ho_SelectedRegions1, out ho_ROIOuter, 8.5);
                ho_ROIInner.Dispose();
                HOperatorSet.ErosionCircle(ho_SelectedRegions1, out ho_ROIInner, 8.5);
                ho_ROI.Dispose();
                HOperatorSet.Difference(ho_ROIOuter, ho_ROIInner, out ho_ROI);
                ho_ROIEdges.Dispose();
                HOperatorSet.Union1(ho_ROI, out ho_ROIEdges);
                //Reduce the region of interest (domain) to the extracted
                //regions containing the edges.
                ho_RimReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_ROIEdges, out ho_RimReduced);
                //Extract subpixel precise edges
                ho_Edges.Dispose();
                HOperatorSet.EdgesSubPix(ho_RimReduced, out ho_Edges, "canny", 4, 20, 40);
                //分割线段
                ho_ContoursSplit.Dispose();
                HOperatorSet.SegmentContoursXld(ho_Edges, out ho_ContoursSplit, "lines_circles", 5, 4, 2);
                //Select only the contours with length larger than 30 pixels
                ho_RelEdges.Dispose();
                HOperatorSet.SelectContoursXld(ho_ContoursSplit, out ho_RelEdges, "length", 30, 999999, 0, 0);
                ho_UnionContours.Dispose();
                HOperatorSet.UnionAdjacentContoursXld(ho_RelEdges, out ho_UnionContours, 100, 1, "attr_keep");
                HOperatorSet.CountObj(ho_UnionContours, out hv_Number);
                if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                {
                    //未找到Mark点
                    return new PointH();
                }
                HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3, 2, 
                    out hv_Row3, out hv_Column3, out hv_Radius2, out hv_StartPhi2, out hv_EndPhi2, out hv_PointOrder3);
                ho_ContCircle1.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_ContCircle1, hv_Row3, hv_Column3, hv_Radius2, 0, 6.28318, "positive", 1);

                HOperatorSet.SetColor(hv_DispWindown, "red");
                HOperatorSet.DispCircle(hv_DispWindown, hv_Row3, hv_Column3, hv_Radius2);
                //HOperatorSet.SetLineWidth(hv_DispWindown, 3);
                //HOperatorSet.DispObj(ho_ContCircle1, hv_DispWindown);
                return new PointH(hv_Column3[0], hv_Row3[0]);
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Regions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionFillUp2.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_ROIOuter.Dispose();
                ho_ROIInner.Dispose();
                ho_ROI.Dispose();
                ho_ROIEdges.Dispose();
                ho_RimReduced.Dispose();
                ho_Edges.Dispose();
                ho_ContoursSplit.Dispose();
                ho_RelEdges.Dispose();
                ho_UnionContours.Dispose();
                ho_ContCircle1.Dispose();
                throw HDevExpDefaultException;
            }
            finally
            {
                ho_Regions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionFillUp2.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_ROIOuter.Dispose();
                ho_ROIInner.Dispose();
                ho_ROI.Dispose();
                ho_ROIEdges.Dispose();
                ho_RimReduced.Dispose();
                ho_Edges.Dispose();
                ho_ContoursSplit.Dispose();
                ho_RelEdges.Dispose();
                ho_UnionContours.Dispose();
                ho_ContCircle1.Dispose();
            }
        }

        #region 九点校正
        /// <summary>
        /// 视野内的点
        /// </summary>
        /// <param name="ho_Image"></param>
        /// <param name="hv_Row"></param>
        /// <param name="hv_Column"></param>
        public void Get9Points(HObject ho_Image, out HTuple hv_Row, out HTuple hv_Column, out HTuple hv_Height, out HTuple hv_Width)
        {
            HTuple hv_Area = null;
            // Local iconic variables 
            HObject ho_Regions, ho_RegionClosing, ho_RegionOpening;
            HObject ho_ConnectedRegions, ho_SelectedRegions1, ho_RegionFillUp1;
            HObject ho_SortedRegions, ho_SelectedRegions2, ho_RegionFillUp2;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_SortedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp2);

            try
            {
                ho_Regions.Dispose();
                //HOperatorSet.AutoThreshold(ho_Image, out ho_Regions, 2);
                HOperatorSet.Threshold(ho_Image, out ho_Regions, 0, 128);
                //ho_RegionClosing.Dispose();
                //HOperatorSet.ClosingCircle(ho_Regions, out ho_RegionClosing, 3.5);
                //ho_RegionOpening.Dispose();
                //HOperatorSet.OpeningCircle(ho_RegionClosing, out ho_RegionOpening, 3.5);
                //ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_ConnectedRegions, out ho_RegionFillUp1);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_RegionFillUp1, out ho_SelectedRegions1, "area", "and", hv_MinAreaPoint, hv_MaxAreaPoint);

                ho_RegionFillUp2.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions1, out ho_RegionFillUp2);
                ho_RegionOpening.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionFillUp2, out ho_RegionOpening, 3.5);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_RegionFillUp2, out ho_RegionOpening, 3.5);

                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_RegionOpening, out ho_RegionFillUp1);
                ho_SortedRegions.Dispose();
                HOperatorSet.SortRegion(ho_RegionFillUp1, out ho_SortedRegions, "upper_left", "true", "row");
                HOperatorSet.AreaCenter(ho_SortedRegions, out hv_Area, out hv_Row, out hv_Column);
                HOperatorSet.RegionFeatures(ho_SortedRegions, "width", out hv_Width);
                HOperatorSet.RegionFeatures(ho_SortedRegions, "height", out hv_Height);                
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Regions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_RegionFillUp1.Dispose();
                ho_SortedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionFillUp2.Dispose();

                throw HDevExpDefaultException;
            }
        }
        //public void Get9Points(HObject ho_Image, out HTuple hv_Row, out HTuple hv_Column, out HTuple hv_Height, out HTuple hv_Width)
        //{
        //    HTuple hv_Area = null;
        //    // Local iconic variables 
        //    HObject ho_Regions, ho_RegionClosing, ho_RegionOpening;
        //    HObject ho_ConnectedRegions, ho_SelectedRegions1, ho_RegionFillUp1;
        //    HObject ho_SortedRegions, ho_SelectedRegions2, ho_RegionFillUp2;

        //    // Initialize local and output iconic variables 
        //    HOperatorSet.GenEmptyObj(out ho_Regions);
        //    HOperatorSet.GenEmptyObj(out ho_RegionClosing);
        //    HOperatorSet.GenEmptyObj(out ho_RegionOpening);
        //    HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
        //    HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
        //    HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
        //    HOperatorSet.GenEmptyObj(out ho_SortedRegions);
        //    HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
        //    HOperatorSet.GenEmptyObj(out ho_RegionFillUp2);

        //    try
        //    {
        //        ho_Regions.Dispose();
        //        //HOperatorSet.AutoThreshold(ho_Image, out ho_Regions, 2);
        //        HOperatorSet.Threshold(ho_Image, out ho_Regions, 0, 128);
        //        ho_RegionClosing.Dispose();
        //        HOperatorSet.ClosingCircle(ho_Regions, out ho_RegionClosing, 3.5);
        //        ho_RegionOpening.Dispose();
        //        HOperatorSet.OpeningCircle(ho_RegionClosing, out ho_RegionOpening, 3.5);
        //        ho_ConnectedRegions.Dispose();
        //        HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions);

        //        ho_SelectedRegions1.Dispose();
        //        HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions1, "area", "and", hv_MinAreaPoint, hv_MaxAreaPoint);
        //        ho_RegionFillUp1.Dispose();
        //        HOperatorSet.FillUp(ho_SelectedRegions1, out ho_RegionFillUp1);
        //        ho_SortedRegions.Dispose();
        //        HOperatorSet.SortRegion(ho_RegionFillUp1, out ho_SortedRegions, "upper_left", "true", "row");
        //        HOperatorSet.AreaCenter(ho_SortedRegions, out hv_Area, out hv_Row, out hv_Column);
        //        HOperatorSet.RegionFeatures(ho_SortedRegions, "width", out hv_Width);
        //        HOperatorSet.RegionFeatures(ho_SortedRegions, "height", out hv_Height);
        //    }
        //    catch (HalconException HDevExpDefaultException)
        //    {
        //        ho_Regions.Dispose();
        //        ho_RegionClosing.Dispose();
        //        ho_RegionOpening.Dispose();
        //        ho_ConnectedRegions.Dispose();
        //        ho_SelectedRegions1.Dispose();
        //        ho_RegionFillUp1.Dispose();
        //        ho_SortedRegions.Dispose();
        //        ho_SelectedRegions2.Dispose();
        //        ho_RegionFillUp2.Dispose();

        //        throw HDevExpDefaultException;
        //    }
        //}
        
        /// <summary>
        /// 像素点尺寸
        /// </summary>
        /// <returns></returns>
        public PointH GetPixelMM()
        {
            HOperatorSet.HomMat2dToAffinePar(hv_HomMat2D, out hv_Sx, out hv_Sy, out hv_Phi, out hv_Theta, out hv_Tx, out hv_Ty);
            double ddsin = Math.Asin(hv_Phi);
            double ddtan = Math.Atan(hv_Phi);
            return new PointH(hv_Sx, hv_Sy);
        }

        #endregion

        #region Halcon显示文字
        // Procedures 
        // Chapter: Graphics / Text
        // Short Description: This procedure writes a text message.
        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
            HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {


            // Local control variables 

            HTuple hv_Red = null, hv_Green = null, hv_Blue = null;
            HTuple hv_Row1Part = null, hv_Column1Part = null, hv_Row2Part = null;
            HTuple hv_Column2Part = null, hv_RowWin = null, hv_ColumnWin = null;
            HTuple hv_WidthWin = null, hv_HeightWin = null, hv_MaxAscent = null;
            HTuple hv_MaxDescent = null, hv_MaxWidth = null, hv_MaxHeight = null;
            HTuple hv_R1 = new HTuple(), hv_C1 = new HTuple(), hv_FactorRow = new HTuple();
            HTuple hv_FactorColumn = new HTuple(), hv_Width = new HTuple();
            HTuple hv_Index = new HTuple(), hv_Ascent = new HTuple();
            HTuple hv_Descent = new HTuple(), hv_W = new HTuple();
            HTuple hv_H = new HTuple(), hv_FrameHeight = new HTuple();
            HTuple hv_FrameWidth = new HTuple(), hv_R2 = new HTuple();
            HTuple hv_C2 = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_CurrentColor = new HTuple();

            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();
            HTuple hv_String_COPY_INP_TMP = hv_String.Clone();

            // Initialize local and output iconic variables 

            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Column: The column coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically
            //   for each new textline.
            //Box: If set to 'true', the text is written within a white box.
            //
            //prepare window
            HOperatorSet.GetRgb(hv_WindowHandle, out hv_Red, out hv_Green, out hv_Blue);
            HOperatorSet.GetPart(hv_WindowHandle, out hv_Row1Part, out hv_Column1Part, out hv_Row2Part,
                out hv_Column2Part);
            HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_RowWin, out hv_ColumnWin,
                out hv_WidthWin, out hv_HeightWin);
            HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightWin - 1, hv_WidthWin - 1);
            //
            //default settings
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
            {
                hv_Color_COPY_INP_TMP = "";
            }
            //
            hv_String_COPY_INP_TMP = ((("" + hv_String_COPY_INP_TMP) + "")).TupleSplit("\n");
            //
            //Estimate extentions of text depending on font size.
            HOperatorSet.GetFontExtents(hv_WindowHandle, out hv_MaxAscent, out hv_MaxDescent,
                out hv_MaxWidth, out hv_MaxHeight);
            if ((int)(new HTuple(hv_CoordSystem.TupleEqual("window"))) != 0)
            {
                hv_R1 = hv_Row_COPY_INP_TMP.Clone();
                hv_C1 = hv_Column_COPY_INP_TMP.Clone();
            }
            else
            {
                //transform image to window coordinates
                hv_FactorRow = (1.0 * hv_HeightWin) / ((hv_Row2Part - hv_Row1Part) + 1);
                hv_FactorColumn = (1.0 * hv_WidthWin) / ((hv_Column2Part - hv_Column1Part) + 1);
                hv_R1 = ((hv_Row_COPY_INP_TMP - hv_Row1Part) + 0.5) * hv_FactorRow;
                hv_C1 = ((hv_Column_COPY_INP_TMP - hv_Column1Part) + 0.5) * hv_FactorColumn;
            }
            //
            //display text box depending on text size
            if ((int)(new HTuple(hv_Box.TupleEqual("true"))) != 0)
            {
                //calculate box extents
                hv_String_COPY_INP_TMP = (" " + hv_String_COPY_INP_TMP) + " ";
                hv_Width = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    HOperatorSet.GetStringExtents(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index), out hv_Ascent, out hv_Descent, out hv_W, out hv_H);
                    hv_Width = hv_Width.TupleConcat(hv_W);
                }
                hv_FrameHeight = hv_MaxHeight * (new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    ));
                hv_FrameWidth = (((new HTuple(0)).TupleConcat(hv_Width))).TupleMax();
                hv_R2 = hv_R1 + hv_FrameHeight;
                hv_C2 = hv_C1 + hv_FrameWidth;
                //display rectangles
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                HOperatorSet.SetColor(hv_WindowHandle, "light gray");
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1 + 3, hv_C1 + 3, hv_R2 + 3, hv_C2 + 3);
                HOperatorSet.SetColor(hv_WindowHandle, "white");
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1, hv_C1, hv_R2, hv_C2);
                HOperatorSet.SetDraw(hv_WindowHandle, hv_DrawMode);
            }
            else if ((int)(new HTuple(hv_Box.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Box";
                throw new HalconException(hv_Exception);
            }
            //Write text.
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_CurrentColor = hv_Color_COPY_INP_TMP.TupleSelect(hv_Index % (new HTuple(hv_Color_COPY_INP_TMP.TupleLength()
                    )));
                if ((int)((new HTuple(hv_CurrentColor.TupleNotEqual(""))).TupleAnd(new HTuple(hv_CurrentColor.TupleNotEqual(
                    "auto")))) != 0)
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_CurrentColor);
                }
                else
                {
                    HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                }
                hv_Row_COPY_INP_TMP = hv_R1 + (hv_MaxHeight * hv_Index);
                HOperatorSet.SetTposition(hv_WindowHandle, hv_Row_COPY_INP_TMP, hv_C1);
                HOperatorSet.WriteString(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                    hv_Index));
            }
            //reset changed window settings
            HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
            HOperatorSet.SetPart(hv_WindowHandle, hv_Row1Part, hv_Column1Part, hv_Row2Part,
                hv_Column2Part);

            return;
        }

        // Chapter: Graphics / Text
        // Short Description: Set font independent of OS
        public void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {


            // Local control variables 

            HTuple hv_OS = null;
            HTuple hv_Exception = new HTuple(), hv_BoldString = new HTuple();
            HTuple hv_SlantString = new HTuple(), hv_AllowedFontSizes = new HTuple();
            HTuple hv_Distances = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_Fonts = new HTuple(), hv_FontSelRegexp = new HTuple();
            HTuple hv_FontsCourier = new HTuple();

            HTuple hv_Bold_COPY_INP_TMP = hv_Bold.Clone();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();
            HTuple hv_Slant_COPY_INP_TMP = hv_Slant.Clone();

            // Initialize local and output iconic variables 

            //This procedure sets the text font of the current window with
            //the specified attributes.
            //It is assumed that following fonts are installed on the system:
            //Windows: Courier New, Arial Times New Roman
            //Mac OS X: CourierNewPS, Arial, TimesNewRomanPS
            //Linux: courier, helvetica, times
            //Because fonts are displayed smaller on Linux than on Windows,
            //a scaling factor of 1.25 is used the get comparable results.
            //For Linux, only a limited number of font sizes is supported,
            //to get comparable results, it is recommended to use one of the
            //following sizes: 9, 11, 14, 16, 20, 27
            //(which will be mapped internally on Linux systems to 11, 14, 17, 20, 25, 34)
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            // dev_get_preferences(...); only in hdevelop
            // dev_set_preferences(...); only in hdevelop
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Set font on Windows systems
                if ((int)((new HTuple((new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))).TupleOr(
                    new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))))).TupleOr(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(
                    "courier")))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Courier New";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Arial";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Times New Roman";
                }
                if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = 1;
                }
                else if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = 0;
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_Slant_COPY_INP_TMP = 1;
                }
                else if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Slant_COPY_INP_TMP = 0;
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                try
                {
                    HOperatorSet.SetFont(hv_WindowHandle, ((((((("-" + hv_Font_COPY_INP_TMP) + "-") + hv_Size_COPY_INP_TMP) + "-*-") + hv_Slant_COPY_INP_TMP) + "-*-*-") + hv_Bold_COPY_INP_TMP) + "-");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //throw (Exception)
                }
            }
            else if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Dar"))) != 0)
            {
                //Set font on Mac OS X systems
                if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_BoldString = "Bold";
                }
                else if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_BoldString = "";
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_SlantString = "Italic";
                }
                else if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_SlantString = "";
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                if ((int)((new HTuple((new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))).TupleOr(
                    new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))))).TupleOr(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(
                    "courier")))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "CourierNewPS";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Arial";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "TimesNewRomanPS";
                }
                if ((int)((new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))).TupleOr(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual(
                    "true")))) != 0)
                {
                    hv_Font_COPY_INP_TMP = ((hv_Font_COPY_INP_TMP + "-") + hv_BoldString) + hv_SlantString;
                }
                hv_Font_COPY_INP_TMP = hv_Font_COPY_INP_TMP + "MT";
                try
                {
                    HOperatorSet.SetFont(hv_WindowHandle, (hv_Font_COPY_INP_TMP + "-") + hv_Size_COPY_INP_TMP);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //throw (Exception)
                }
            }
            else
            {
                //Set font for UNIX systems
                hv_Size_COPY_INP_TMP = hv_Size_COPY_INP_TMP * 1.25;
                hv_AllowedFontSizes = new HTuple();
                hv_AllowedFontSizes[0] = 11;
                hv_AllowedFontSizes[1] = 14;
                hv_AllowedFontSizes[2] = 17;
                hv_AllowedFontSizes[3] = 20;
                hv_AllowedFontSizes[4] = 25;
                hv_AllowedFontSizes[5] = 34;
                if ((int)(new HTuple(((hv_AllowedFontSizes.TupleFind(hv_Size_COPY_INP_TMP))).TupleEqual(
                    -1))) != 0)
                {
                    hv_Distances = ((hv_AllowedFontSizes - hv_Size_COPY_INP_TMP)).TupleAbs();
                    HOperatorSet.TupleSortIndex(hv_Distances, out hv_Indices);
                    hv_Size_COPY_INP_TMP = hv_AllowedFontSizes.TupleSelect(hv_Indices.TupleSelect(
                        0));
                }
                if ((int)((new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))).TupleOr(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(
                    "Courier")))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "courier";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "helvetica";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "times";
                }
                if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = "bold";
                }
                else if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = "medium";
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("times"))) != 0)
                    {
                        hv_Slant_COPY_INP_TMP = "i";
                    }
                    else
                    {
                        hv_Slant_COPY_INP_TMP = "o";
                    }
                }
                else if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Slant_COPY_INP_TMP = "r";
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                try
                {
                    HOperatorSet.SetFont(hv_WindowHandle, ((((((("-adobe-" + hv_Font_COPY_INP_TMP) + "-") + hv_Bold_COPY_INP_TMP) + "-") + hv_Slant_COPY_INP_TMP) + "-normal-*-") + hv_Size_COPY_INP_TMP) + "-*-*-*-*-*-*-*");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    if ((int)((new HTuple(((hv_OS.TupleSubstr(0, 4))).TupleEqual("Linux"))).TupleAnd(
                        new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("courier")))) != 0)
                    {
                        HOperatorSet.QueryFont(hv_WindowHandle, out hv_Fonts);
                        hv_FontSelRegexp = (("^-[^-]*-[^-]*[Cc]ourier[^-]*-" + hv_Bold_COPY_INP_TMP) + "-") + hv_Slant_COPY_INP_TMP;
                        hv_FontsCourier = ((hv_Fonts.TupleRegexpSelect(hv_FontSelRegexp))).TupleRegexpMatch(
                            hv_FontSelRegexp);
                        if ((int)(new HTuple((new HTuple(hv_FontsCourier.TupleLength())).TupleEqual(
                            0))) != 0)
                        {
                            hv_Exception = "Wrong font name";
                            //throw (Exception)
                        }
                        else
                        {
                            try
                            {
                                HOperatorSet.SetFont(hv_WindowHandle, (((hv_FontsCourier.TupleSelect(
                                    0)) + "-normal-*-") + hv_Size_COPY_INP_TMP) + "-*-*-*-*-*-*-*");
                            }
                            // catch (Exception) 
                            catch (HalconException HDevExpDefaultException2)
                            {
                                HDevExpDefaultException2.ToHTuple(out hv_Exception);
                                //throw (Exception)
                            }
                        }
                    }
                    //throw (Exception)
                }
            }
            // dev_set_preferences(...); only in hdevelop

            return;
        }
        #endregion


        /// <summary>
        /// 模板中心补偿
        /// </summary>
        /// <param name="offset">补偿值</param>
        /// <param name="Center">模板中心坐标</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="isClockwise">true:顺时针(默认)/false:逆时针</param>
        /// <returns>旋转后坐标</returns>
        public static PointH DeviationCalc(PointH offset, PointH Center, double angle)
        {
            try
            {
                if (angle > 180) angle -= 360;
                PointH _P = new PointH(offset.X, offset.Y);      //待旋转点
                PointH _Center = new PointH(Center.X, Center.Y); //旋转中心
                bool isClockwise = (angle >= 0 && angle <= 180) ? false : true; //顺时、逆时针旋转
                double rad = Math.PI / 180 * (isClockwise ? -angle : angle);
                PointH newP = RotatePoint(_P, _Center, rad, isClockwise);
                PointH point = new PointH((float)newP.X, (float)newP.Y);
                return point; //Y轴坐标系再取反
            }
            catch { return new PointH(); }
        }

        /// <summary>
        /// 计算点P(x,y)与X轴正方向的夹角
        /// </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        /// <returns>夹角弧度</returns>
        private static double radPOX(double x, double y)
        {
            //P在(0,0)的情况
            if (x == 0 && y == 0) return 0;

            //P在四个坐标轴上的情况：x正、x负、y正、y负
            if (y == 0 && x > 0) return 0;
            if (y == 0 && x < 0) return Math.PI;
            if (x == 0 && y > 0) return Math.PI / 2;
            if (x == 0 && y < 0) return Math.PI / 2 * 3;

            //点在第一、二、三、四象限时的情况
            if (x > 0 && y > 0) return Math.Atan(y / x);
            if (x < 0 && y > 0) return Math.PI - Math.Atan(y / -x);
            if (x < 0 && y < 0) return Math.PI + Math.Atan(-y / -x);
            if (x > 0 && y < 0) return Math.PI * 2 - Math.Atan(-y / x);

            return 0;
        }
        /// <summary>
        /// 返回点P围绕点A旋转弧度rad后的坐标
        /// </summary>
        /// <param name="P">待旋转点坐标</param>
        /// <param name="Center">旋转中心坐标</param>
        /// <param name="rad">旋转弧度</param>
        /// <param name="isClockwise">true:顺时针(默认)/false:逆时针</param>
        /// <returns>旋转后坐标</returns>
        private static PointH RotatePoint(PointH P, PointH Center,
            double rad, bool isClockwise = true)
        {
            //点Temp1
            PointH Temp1 = new PointH(P.X - Center.X, P.Y - Center.Y);
            //点Temp1到原点的长度
            double lenO2Temp1 = Temp1.DistanceTo(new PointH(0, 0));
            //∠T1OX弧度
            double angT1OX = radPOX(Temp1.X, Temp1.Y);
            //∠T2OX弧度（T2为T1以O为圆心旋转弧度rad）
            double angT2OX = angT1OX - (isClockwise ? 1 : -1) * rad;
            //点Temp2
            PointH Temp2 = new PointH(
                lenO2Temp1 * Math.Cos(angT2OX),
                lenO2Temp1 * Math.Sin(angT2OX));
            //点Q
            return new PointH(Temp2.X + Center.X, Temp2.Y + Center.Y);
        }

    }

    public struct PointH
    {        
        //横、纵坐标
        private double x, y;

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double X
        {
            get { return x; }
            set { x = value; }
        }
        //构造函数
        public PointH(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        
        //该点到指定点pTarget的距离
        public double DistanceTo(PointH p)
        {
            return Math.Sqrt((p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y));
        }
    }

}
