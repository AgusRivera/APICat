using APICat.Application.Models.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Application.Validators
{
    public class CatValidator : AbstractValidator<BreedsDto>
    {
        public CatValidator()
        {
            RuleFor(data => data.Name).NotEmpty().WithMessage("El campo NAME no puede ser nulo")
                                      .MaximumLength(200).WithMessage("El campo NAME tiene una longitud máxima de 200 caracteres");

            RuleFor(data => data.Description).NotEmpty().WithMessage("El campo DESCRIPTION no puede ser nulo")
                                             .MaximumLength(200).WithMessage("El campo DESCRIPTION tiene una longitud máxima de 200 caracteres");

            RuleFor(data => data.Temperament).NotEmpty().WithMessage("El campo TEMPERAMENT no puede ser nulo")
                                             .MaximumLength(300).WithMessage("El campo TEMPERAMENT tiene una longitud máxima de 300 caracteres");
            
            RuleFor(data => data.Origin).NotEmpty().WithMessage("El campo ORIGIN no puede ser nulo")
                                        .MaximumLength(200).WithMessage("El campo ORIGIN tiene una longitud máxima de 200 caracteres");

            RuleFor(data => data.Wikipedia_url).NotEmpty().WithMessage("El campo URL no puede ser nulo")
                                               .WithMessage("El campo URL tiene una longitud maxima de 100 caracteres").MaximumLength(100);
        }
    }
}
