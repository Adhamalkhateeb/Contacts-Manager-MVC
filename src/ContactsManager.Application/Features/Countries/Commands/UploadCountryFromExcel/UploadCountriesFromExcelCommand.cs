using System;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public record UploadCountriesFromExcelCommand(IFormFile file) : IRequest<Result<int>> { }
