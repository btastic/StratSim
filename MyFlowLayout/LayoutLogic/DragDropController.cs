﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyFlowLayout
{
    enum DragState { Drag, Drop }

    /// <summary>
    /// <para>Provides methods for displaying and controlling 
    /// dynamic control re-ordering.</para>
    /// <para>Provides a panel that can be displayed on the screen to show the layout.</para>
    /// </summary>
    public class DragDropController : Panel
    {
        List<MyPanel> visiblePanels;
        MyPanel panelBeingRelocated;
        Point startLocation;

        Dictionary<DockTypes, Point> dockPoints;

        DragState dragState;

        //Required graphics class-level variables
        DockTypes dockPointToHighlight;
        List<Rectangle> rectanglesToDraw = new List<Rectangle>();
        Rectangle panelRectangle;

        MainForm form;
        MainFormCollection collection;

        public DragDropController(MainFormCollection Collection)
        {
            this.DoubleBuffered = true;
            this.collection = Collection;
            LinkFormsToStartDragDrop();
        }

        void LinkFormsToStartDragDrop()
        {
            MyFlowLayout.MainFormIOController.StartLayout += StartDragDropLayout;
        }
        void UnlinkFormsFromStartDragDrop()
        {
            MyFlowLayout.MainFormIOController.StartLayout -= StartDragDropLayout;
        }

        /// <summary>
        /// Starts the dynamic drag-drop process on the form by displaying the drag-drop interface.
        /// Events are subscribed to so that when the panel is clicked again the panel is dropped.
        /// </summary>
        /// <param name="panelBeingDragged">The panel that has been selected</param>
        /// <param name="visibleControls">A list of all visible panels on the form</param>
        /// <param name="startLocation">The initial click location</param>
        /// <param name="dockPoints">A dictionary of the available points the panel can be docked to on the form</param>
        public void StartDragDropLayout(MyPanel panelBeingDragged, List<MyPanel> visibleControls, Point startLocation, Dictionary<DockTypes,Point> dockPoints)
        {
            UnlinkFormsFromStartDragDrop();
            this.visiblePanels = visibleControls;
            this.panelBeingRelocated = panelBeingDragged;
            this.startLocation = startLocation;
            this.dockPoints = dockPoints;

            MouseMove += DragDropController_MouseMove;
            MouseUp += DragDropController_MouseUp;

            form = panelBeingDragged.ParentForm;
            collection.AddEventToForms(DropPanelOnCurrentForm);
            IOController.FlowLayoutEvents.PanelDrag += MyEvents_PanelDrag;

            rectanglesToDraw.Clear();

            dragState = DragState.Drag;

            StartLayout();

            IOController.ShowDynamicLayoutPanel(this);
        }

        void DragDropController_MouseUp(object sender, MouseEventArgs e)
        {
            dragState = DragState.Drop;
            DropPanel(e.Location);
        }

        void DragDropController_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragState == DragState.Drag)
                IOController.FlowLayoutEvents.OnPanelDrag(e.Location);
        }

        void MyEvents_PanelDrag(Point newPoint)
        {
            panelRectangle.Location = new Point(newPoint.X, newPoint.Y);
            dockPointToHighlight = FindNearestDockPoint(newPoint);
            Invalidate();
        }

        void StartLayout()
        {
            foreach (MyPanel p in visiblePanels)
            {
                if (p == panelBeingRelocated)
                {
                    panelRectangle = new Rectangle(p.Location, p.OriginalSize);
                }
                else
                {
                    rectanglesToDraw.Add(new Rectangle(p.Location, p.Size));
                }
            }

            Invalidate();
        }

        /// <summary>
        /// Unsubscribes from events and hides the layout panel, returning to the original window state.
        /// </summary>
        void FinishLayout()
        {
            IOController.HideDynamicLayoutPanel(this);

            MouseMove -= DragDropController_MouseMove;
            MouseUp -= DragDropController_MouseUp;
            IOController.FlowLayoutEvents.PanelDrag -= MyEvents_PanelDrag;
            collection.RemoveEventFromForms(DropPanelOnCurrentForm);
            LinkFormsToStartDragDrop();
        }

        /// <summary>
        /// Drops the panel at the nearest dock point to the point where it has been dropped.
        /// </summary>
        /// <param name="LocationToDrop">A point representing the coordinates of the location that the panel was dropped at</param>
        void DropPanel(Point LocationToDrop)
        {
            //Setting the dock type fires the re-layout event
            bool openInNewWindow = false;
            panelBeingRelocated.DockType = FindNearestDockPoint(LocationToDrop, out openInNewWindow);

            if (openInNewWindow)
                collection.OpenInNewWindow(panelBeingRelocated, panelBeingRelocated.ParentForm);

            FinishLayout();
        }

        void DropPanelOnCurrentForm(MainForm formPanelIsDroppedOn)
        {
            dragState = DragState.Drop;
            IOController.OpenPanelInCurrentForm(panelBeingRelocated, formPanelIsDroppedOn);
            FinishLayout();
        }

        /// <summary>
        /// Finds the dock point closest to the specified point.
        /// </summary>
        /// <param name="p">The point that is being searched</param>
        /// <returns>A dock type specifying the type of dock point that is closest to the specified point.</returns>
        DockTypes FindNearestDockPoint(Point p)
        {
            DockTypes nearestDockType = panelBeingRelocated.DockType;

            int lowestDifference = 0;
            int currentDifference = 0;

            foreach (var dockType in (DockTypes[])Enum.GetValues(typeof(DockTypes)))
            {
                currentDifference = (int)Math.Pow((dockPoints[dockType].X - p.X), 2) + (int)Math.Pow((dockPoints[dockType].Y - p.Y), 2);

                if ((int)dockType == 0)
                    lowestDifference = currentDifference;

                if (currentDifference <= lowestDifference)
                {
                    lowestDifference = currentDifference;
                    nearestDockType = dockType;
                }
            }

            return nearestDockType;
        }

        /// <summary>
        /// Finds the nearest dock point to the specified point.
        /// </summary>
        /// <param name="p">The point that is being searched</param>
        /// <param name="openInNewWindow"><para>Set to true if the panel should be opened in a new window.</para>
        /// <para>This occurs when the panel is positioned away from any dock points.</para></param>
        /// <returns></returns>
        DockTypes FindNearestDockPoint(Point p, out bool openInNewWindow)
        {
            DockTypes nearestDockType = panelBeingRelocated.DockType;

            int lowestDifference = 0;
            int currentDifference = 0;
            openInNewWindow = false;

            foreach (var dockType in (DockTypes[])Enum.GetValues(typeof(DockTypes)))
            {
                currentDifference = (int)Math.Pow((dockPoints[dockType].X - p.X), 2) + (int)Math.Pow((dockPoints[dockType].Y - p.Y), 2);

                if ((int)dockType == 0)
                    lowestDifference = currentDifference;

                if (currentDifference <= lowestDifference)
                {
                    lowestDifference = currentDifference;
                    nearestDockType = dockType;
                }
            }

            //If not near any dock points, the panel will be opened in a new window.
            //The dock point is set to the top left so the panel appears full screen.
            if (lowestDifference >= 65000)
            {
                openInNewWindow = true;
                nearestDockType = DockTypes.TopLeft;
            }

            return nearestDockType;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var unusedPanelsPen = new Pen(Color.Black, 1F);
            var locatePanelsPen = new Pen(Color.Red, 1F);
            var dockPointsPen = new Pen(Color.Blue, 2F);
            var highlightPointsPen = new Pen(Color.LightBlue, 2F);

            base.OnPaint(e);

            foreach (var dockType in (DockTypes[])Enum.GetValues(typeof(DockTypes)))
            {
                g.DrawEllipse((dockType == dockPointToHighlight ? highlightPointsPen : dockPointsPen), (int)dockPoints[dockType].X - 10, (int)dockPoints[dockType].Y - 10, 20, 20);
            }

            foreach (Rectangle r in rectanglesToDraw)
            {
                g.DrawRectangle(unusedPanelsPen, r);
            }
            g.DrawRectangle(locatePanelsPen, panelRectangle);

            unusedPanelsPen.Dispose();
            locatePanelsPen.Dispose();
            dockPointsPen.Dispose();
            highlightPointsPen.Dispose();
        }

        protected override void Dispose(bool disposing)
        { /* Do not dispose as it will switch associated forms */ }

        MainFormIOController IOController
        { get { return form.IOController; } }

    }
}