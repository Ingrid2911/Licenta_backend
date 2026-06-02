using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using time_expanded_graph.Models.Building;
using time_expanded_graph.View.Dialogs;  // Add this using directive

namespace time_expanded_graph.View.Drawing.FloorPlan.Interaction
{
    public class ContextMenuHandler
    {
        private readonly BuildingPlan _plan;

        public event Action? PlanChanged;
        public event Action? RedrawRequested;

        public ContextMenuHandler(BuildingPlan plan)
        {
            _plan = plan;
        }

        public void ShowContextMenu(BuildingElement el, Canvas canvas)
        {
            var menu = new ContextMenu();

            // Header
            var header = new MenuItem
            {
                Header = $"{el.Type}: {el.Id}  (cap:{el.Capacity} t:{el.TravelTime})",
                IsEnabled = false,
                FontWeight = FontWeights.Bold
            };
            menu.Items.Add(header);
            menu.Items.Add(new Separator());

            // Rename (only for rooms)
            if (el.Type == BuildingElementType.Room)
            {
                AddRenameMenuItem(menu, el);
                menu.Items.Add(new Separator());
            }

            // Edit connections
            var conns = _plan.Connections
                .Where(c => c.FromId == el.Id || c.ToId == el.Id)
                .ToList();

            if (conns.Count > 0)
            {
                AddConnectionMenuItems(menu, el, conns);
                menu.Items.Add(new Separator());
            }

            // Delete element
            AddDeleteMenuItem(menu, el);

            canvas.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void AddRenameMenuItem(ContextMenu menu, BuildingElement el)
        {
            var editLabel = new MenuItem { Header = "Redenumește camera..." };
            editLabel.Click += (s, e) =>
            {
                var dlg = new RenameDialog(el.Label);  // Now RenameDialog is found
                if (dlg.ShowDialog() == true)
                {
                    el.Label = dlg.NewName;
                    RedrawRequested?.Invoke();
                    PlanChanged?.Invoke();
                }
            };
            menu.Items.Add(editLabel);
        }

        private void AddConnectionMenuItems(ContextMenu menu, BuildingElement el,
            System.Collections.Generic.List<HallwayConnection> conns)
        {
            var connMenu = new MenuItem { Header = $"Conexiuni ({conns.Count})" };

            foreach (var conn in conns)
            {
                string other = conn.FromId == el.Id ? conn.ToId : conn.FromId;
                var sub = new MenuItem
                {
                    Header = $"↔ {other}  cap:{conn.Capacity} t:{conn.TravelTime}"
                };

                AddCapacityTimeMenuItems(sub, conn);

                // Delete connection
                sub.Items.Add(new Separator());
                var del = new MenuItem { Header = "Șterge conexiunea" };
                del.Click += (s, e) =>
                {
                    _plan.RemoveConnection(conn);
                    RedrawRequested?.Invoke();
                    PlanChanged?.Invoke();
                };
                sub.Items.Add(del);

                connMenu.Items.Add(sub);
            }

            menu.Items.Add(connMenu);
        }

        private void AddCapacityTimeMenuItems(MenuItem parent, HallwayConnection conn)
        {
            AddMenuItem(parent, "Capacitate  +1", () => conn.Capacity++);
            AddMenuItem(parent, "Capacitate  −1", () =>
            {
                if (conn.Capacity > 1) conn.Capacity--;
            });
            parent.Items.Add(new Separator());
            AddMenuItem(parent, "TravelTime +1", () => conn.TravelTime++);
            AddMenuItem(parent, "TravelTime −1", () =>
            {
                if (conn.TravelTime > 1) conn.TravelTime--;
            });
        }

        private void AddDeleteMenuItem(ContextMenu menu, BuildingElement el)
        {
            var deleteItem = new MenuItem { Header = "🗑  Șterge elementul" };
            deleteItem.Click += (s, e) =>
            {
                _plan.RemoveElement(el);
                RedrawRequested?.Invoke();
                PlanChanged?.Invoke();
            };
            menu.Items.Add(deleteItem);
        }

        private void AddMenuItem(MenuItem parent, string header, Action action)
        {
            var menuItem = new MenuItem { Header = header };
            menuItem.Click += (s, e) =>
            {
                action();
                RedrawRequested?.Invoke();
                PlanChanged?.Invoke();
            };
            parent.Items.Add(menuItem);
        }
    }
}