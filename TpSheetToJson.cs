using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace TpSheetToJson2
{
	
    class TpSheetToJson
    {
		public Form1 form1;
				
        public JObject ConvertTpSheetToJson(string inputTxt, string subfix = "")
        {
			var reader = new StringReader(inputTxt);
			string line = "";
			int canvasW = 0;
			int canvasH = 0;
            JObject root, meta;
            List<JObject> frames = new List<JObject>();

			string image = "";
			string smartupdate = "";
			JObject size = new JObject();

			Regex regSmartUpdate = new Regex(@"\A# (?<smartupdate>\$.*\$)\Z");
			Regex regMeta = new Regex(@"\A:(?<metaName>[a-z]*)=(?<metaValue>.*)\Z");
			Regex regSize = new Regex(@"(?<sizeW>\d*)x(?<sizeH>\d*)\Z");
			Regex regFrame = new Regex(@"\A(?<filename>[a-zA-Z0-9_\-]*);(?<x>\d*);(?<y>\d*);(?<w>\d*);(?<h>\d*); (?<pivotX>\-?\d*\.?\d*);(?<pivotY>\-?\d*\.?\d*); 0;0;0;0");
			Regex regTexName = new Regex(@"\A(?<texName>.*)(?<texExt>\.[a-zA-Z0-9]*)\Z");

			var lineCount = 0;
			while ( (line = reader.ReadLine() ) != null)
            {
				lineCount++;
				form1.txtChanger(lineCount.ToString());
				if (regSmartUpdate.IsMatch(line))
                {
					form1.txtChanger("GetUpdate");
					var match = regSmartUpdate.Match(line);
					smartupdate = match.Groups["smartupdate"].Value;
				}
				else if (regMeta.IsMatch(line))
                {
					form1.txtChanger("GetMeta");
					var match = regMeta.Match(line);
					var metaName = match.Groups["metaName"].Value;
					var metaValue = match.Groups["metaValue"].Value;
                    switch (metaName)
                    {
						case "texture":
							var match3 = regTexName.Match(metaValue);
							if (match3.Success == false)
								form1.txtChanger("Texture檔名比對失敗");
							var texName = match3.Groups["texName"].Value;
							var texExt = match3.Groups["texExt"].Value;
							image = texName + subfix + texExt;
							break;
						case "size":
							var match2 = regSize.Match(metaValue);
							canvasW = Int32.Parse(match2.Groups["sizeW"].Value);
							canvasH = Int32.Parse(match2.Groups["sizeH"].Value);
							size = new JObject(
								new JProperty("w", canvasW),
								new JProperty("h", canvasH)
							);
							break;
						default:
							break;
					}
                }
				else if (regFrame.IsMatch(line))
                {
					form1.txtChanger("getFrames");
					var match = regFrame.Match(line);
					frames.Add( GetFrameData(match, canvasW, canvasH, subfix) );
				}
            }

			meta = new JObject(
				new JProperty("app", @"https://www.codeandweb.com/texturepacker"),
				new JProperty("version", "1.0"),
				new JProperty("image", image),
				new JProperty("format", "RGBA8888"),
				new JProperty("size", size),
				new JProperty("scale", "1"),
				new JProperty("smartupdate", smartupdate)
			);

			root = new JObject(
				new JProperty("frames", frames.ToArray()),
				new JProperty("meta", meta)
			);

			return root;
        }

        private JObject GetFrameData(Match match, int spriteSheetW, int spriteSheetH, string subfix = "")
        {
            var filename = match.Groups["filename"].Value + subfix;

			int x, y, w, h;
			x = Int32.Parse(match.Groups["x"].Value);
			w = Int32.Parse(match.Groups["w"].Value);
			h = Int32.Parse(match.Groups["h"].Value);			
			y = spriteSheetH - Int32.Parse(match.Groups["y"].Value) - h;
			JObject frameJob = new JObject(
				new JProperty("x", x),
				new JProperty("y", y),
				new JProperty("w", w),
				new JProperty("h", h)
			);

			double pivotX, pivotY;
			pivotX = double.Parse(match.Groups["pivotX"].Value);
			pivotY = double.Parse(match.Groups["pivotY"].Value);		
			JObject pivotJob = new JObject(
				new JProperty("x", pivotX),
				new JProperty("y", pivotY)
			);

			//算出pivot到左側與到右側的距離，用較長的一邊算出畫布大小
			double pivot2L, pivot2R, pivot2U, pivot2D;
			pivot2L = Math.Round(w * pivotX);
			pivot2R = w - pivot2L;
			pivot2D = Math.Round(h * pivotY);
			pivot2U = h - pivot2D;

			int canvasX, canvasY, canvasW, canvasH;
			
			canvasY = 0;
			if (pivot2L >= pivot2R)
            {
				canvasX = 0;
				canvasW = (int)pivot2L * 2;

			}
            else
            {
				canvasW = (int)pivot2R * 2;
				canvasX = (int)(pivot2R - pivot2L);
			}
			if(pivot2U >= pivot2D)
            {
				canvasY = 0;
				canvasH = (int)pivot2U * 2;
            }
            else
            {
				canvasY = (int)(pivot2D - pivot2U);
				canvasH = (int)pivot2D * 2;
			}

			JObject spriteSourceSize = new JObject(
				new JProperty("x", canvasX),
				new JProperty("y", canvasY),
				new JProperty("w", w),
				new JProperty("h", h)
			);

			JObject sourceSize = new JObject(
				new JProperty("w", canvasW),
				new JProperty("h", canvasH)
			);

			JObject job = new JObject(
				new JProperty("filename", filename),
				new JProperty("frame", frameJob),
				new JProperty("rotated", false),
				new JProperty("trimmed", true),
				new JProperty("spriteSourceSize", spriteSourceSize),
				new JProperty("sourceSize", sourceSize),
				new JProperty("pivot", pivotJob)
			);

			return job;
        }

    }
}
