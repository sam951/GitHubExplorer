using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubExplorer.Contracts.DTO;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PerPage, int TotalCount);
