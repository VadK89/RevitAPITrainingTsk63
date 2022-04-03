using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevitAPITrainingTsk63
{

    /*Создайте приложение, которое позволяет расставлять определенное количество элементов
модели между двумя точками. Приложение должно работать так: приложение
запрашивает начальную и конечную точку. Далее появляется окно со списком типов
семейств и количество элементов, которое установится между указанными точками.

Отправьте файл приложения и модели на проверку.
*/
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<FamilySymbol> ElementTypes { get; } = new List<FamilySymbol>();//+сразу инициализация списка в классе
        public FamilySymbol SelectedElementType { get; set; }

        public int ElementNum { get; set; }
        public DelegateCommand SaveCommand { get; }
        public List<XYZ> Points { get; set; } = new List<XYZ>();

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            ElementTypes = FamilySymbolUtils.GetFamilySymbols(commandData);
            ElementNum = 1;
            SaveCommand = new DelegateCommand(OnSaveCommand);
            //Points = SelectionUtils.GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            XYZ startPoint = null;
            XYZ endPoint = null;


            if (ElementNum < 1)
            {
                MessageBox.Show("Уточнить количество");
                return;
            }
            List<XYZ> PointElement = new List<XYZ>();
            if (ElementNum == 1)
            {
                RaiseCloseRequest();
                Points = SelectionUtils.GetPoints(_commandData, "Выбор точки", ObjectSnapTypes.Endpoints);
                PointElement.Add(Points[0]);

            }
            else if (ElementNum > 1)
            {
                RaiseCloseRequest();
                Points = SelectionUtils.GetPoints(_commandData, "Выбор точки", ObjectSnapTypes.Endpoints);
                for (int i = 0; i < Points.Count; i++)
                {
                    if (i == 0)
                        continue;
                    //сохранение точек
                    var prevPoint = Points[i - 1];
                    var currentPoint = Points[i];
                    startPoint = prevPoint;
                    endPoint = currentPoint;
                    XYZ length = endPoint - startPoint;


                    for (int j = 0; j <= ElementNum-1; j++)
                    {

                        XYZ step = length / (ElementNum-1);
                        XYZ insertPoint = startPoint + j * step;
                        PointElement.Add(insertPoint);
                    }
                }


            }
            foreach (var pointI in PointElement)
            {
                FamilyInstanceUtils.CreateFamilyInstance(_commandData, SelectedElementType, pointI, doc.ActiveView.GenLevel);
            }


        }


        //для закрытия окна
        public event EventHandler CloseRequest;
        //метод для закрытия окна
        private void RaiseCloseRequest()
        {//Для запуска методов привзязанных к запросу закрытия
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
