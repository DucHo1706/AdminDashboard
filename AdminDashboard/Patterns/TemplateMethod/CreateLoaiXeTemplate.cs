using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Patterns.TemplateMethod
{
    public class CreateLoaiXeTemplate : CreateEntityTemplate<LoaiXe>
    {
        public CreateLoaiXeTemplate(Db27524Context context) : base(context)
        {
        }

        protected override void PrepareModelState(Controller controller)
        {
            controller.ModelState.Remove("LoaiXeId");
        }

        protected override Task<bool> ValidateAsync(Controller controller, LoaiXe model)
        {
            return Task.FromResult(controller.ModelState.IsValid);
        }

        protected override Task GenerateIdAsync(LoaiXe model)
        {
            model.LoaiXeId = Guid.NewGuid().ToString("N").Substring(0, 8);
            return Task.CompletedTask;
        }
    }
}