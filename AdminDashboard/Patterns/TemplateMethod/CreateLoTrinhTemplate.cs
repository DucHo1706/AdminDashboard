using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminDashboard.Patterns.TemplateMethod
{
    public class CreateLoTrinhTemplate : CreateEntityTemplate<LoTrinh>
    {
        public CreateLoTrinhTemplate(Db27524Context context) : base(context)
        {
        }

        protected override void PrepareModelState(Controller controller)
        {
            controller.ModelState.Remove("LoTrinhId");
            controller.ModelState.Remove("TramDiNavigation");
            controller.ModelState.Remove("TramToiNavigation");
        }

        protected override Task LoadViewDataAsync(Controller controller, LoTrinh model)
        {
            controller.ViewData["TramDi"] = new SelectList(_context.Tram, "IdTram", "TenTram", model?.TramDi);
            controller.ViewData["TramToi"] = new SelectList(_context.Tram, "IdTram", "TenTram", model?.TramToi);
            return Task.CompletedTask;
        }

        protected override Task<bool> ValidateAsync(Controller controller, LoTrinh model)
        {
            if (!model.GiaVeCoDinh.HasValue)
            {
                controller.ModelState.AddModelError("GiaVeCoDinh", "Giá vé là bắt buộc và phải lớn hơn hoặc bằng 5,000 VNĐ.");
            }
            else
            {
                decimal value = model.GiaVeCoDinh.Value;

                if (value < 0)
                {
                    controller.ModelState.AddModelError("GiaVeCoDinh", "Giá vé không được là số âm! Vui lòng nhập số dương lớn hơn hoặc bằng 5,000 VNĐ.");
                }
                else if (value < 5000)
                {
                    controller.ModelState.AddModelError("GiaVeCoDinh", "Giá vé phải lớn hơn hoặc bằng 5,000 VNĐ. Vui lòng nhập lại!");
                }
            }

            if (model.TramDi == model.TramToi)
            {
                controller.ModelState.AddModelError("TramToi", "Trạm đến không được trùng với trạm đi.");
            }

            return Task.FromResult(controller.ModelState.IsValid);
        }

        protected override Task GenerateIdAsync(LoTrinh model)
        {
            model.LoTrinhId = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }
    }
}