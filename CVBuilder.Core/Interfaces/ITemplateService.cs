using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVBuilder.Core.Interfaces
{
    public interface ITemplateService
    {
        Task<TemplateDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<string?> GetCssAsync(int id, CancellationToken ct = default);
    }
}
