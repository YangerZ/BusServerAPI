using Dapper;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Spatial;
using System.Threading.Tasks;

namespace WebApplication1.Params
{
    
    public class GeographyTypeMapper : SqlMapper.TypeHandler<Geometry>
    {
        public override void SetValue(IDbDataParameter parameter, Geometry value)
        {
            if (parameter is NpgsqlParameter npgsqlParameter)
            {
                npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Geography;
                npgsqlParameter.NpgsqlValue = value;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public override Geometry Parse(object value)
        {
            if (value is Geometry geometry)
            {
                return geometry;
            }

            throw new ArgumentException();
        }
    }
}
