﻿using System.Collections.Generic;
using System.Linq;

namespace JMMServer.API.Model.common
{
    public class Filter
    {
        public int id { get; set; }
        public string name { get; set; }
        public ArtCollection art { get; set; }
        public int size { get; set; }
        public int viewed { get; set;}
        public string url { get; set; }
        public string type { get; set; }
        public List<Group> groups { get; set; }

        public Filter()
        {
            art = new ArtCollection();
            groups = new List<Group>();
        }

        internal Filter GenerateFromGroupFilter(Entities.GroupFilter gf, int uid, int nocast, int notag, int level, int all)
        {
            Filter filter = new Filter();
            filter.name = gf.GroupFilterName;
            filter.id = gf.GroupFilterID;
            filter.size = 0;
            filter.type = gf.FilterType.ToString();

            if (gf.GroupsIds.ContainsKey(uid))
            {
                HashSet<int> groupsh = gf.GroupsIds[uid];
                if (groupsh.Count != 0)
                {
                    filter.size = groupsh.Count;
                    bool image_found = false;

                    foreach (int gp in groupsh)
                    {
                        Entities.AnimeGroup ag = Repositories.RepoFactory.AnimeGroup.GetByID(gp);

                        if (ag != null)
                        {
                            JMMContracts.PlexAndKodi.Video v = ag.GetPlexContract(uid);
                            
                            if (v.Art != null && !image_found)
                            {
                                filter.art.fanart.Add(new Art() { url = APIHelper.ConstructImageLinkFromRest(v.Art), index = filter.art.fanart.Count });

                                if (v.Banner != null)
                                {
                                    filter.art.banner.Add(new Art() { url = APIHelper.ConstructImageLinkFromRest(v.Banner), index = filter.art.banner.Count });
                                }

                                if (v.Thumb != null)
                                {
                                    filter.art.thumb.Add(new Art() { url = APIHelper.ConstructImageLinkFromRest(v.Thumb), index = filter.art.thumb.Count });
                                }

                                if (filter.art.fanart.Count > 0)
                                {
                                    image_found = true;
                                }
                            }

                            // only scan deeper if the level is correct
                            if (level > 1)
                            {
                                groups.Add(new Group().GenerateFromAnimeGroup(ag, uid, nocast, notag, (level - 1), all));
                            }
                            else
                            {
                                // if we have image for filter and we dont wan't include series inside filter then break
                                if (image_found)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    // save groups
                    if (groups.Count > 0)
                    {
                        filter.groups = groups;
                    }
                }
            }

            filter.viewed = 0;
            filter.url = APIHelper.ConstructFilterIdUrl(filter.id);

            return filter;
        }
    }
}
