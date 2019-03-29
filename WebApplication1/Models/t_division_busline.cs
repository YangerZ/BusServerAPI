using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
namespace WebApplication1.Models
{
    public class t_division_busline
    {
        //CREATE TABLE public.t_division_busline
        //        (
        //            id SERIAL primary key,
        //    gid integer NOT NULL,
        //    lineguid character varying(254) COLLATE pg_catalog."default",
        //    geom geometry(MultiLineStringZM,4326)
        //)
        //WITH(
        //    OIDS = FALSE
        //)
        //TABLESPACE pg_default;
        //CREATE INDEX t_division_busline2_geom_idx1
        //    ON public.t_division_busline USING gist
        //    (geom)
        //    TABLESPACE pg_default;
        //public int  id { get; set; }
        public int gid { get; set; }
        public string lineguid { get; set; }
        public MultiLineString geom { get; set; }
    }
}
