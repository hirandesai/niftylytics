# NiftyLytics
The purpose of the project is to generate analytics for indian stock market. This is simple utility that takes a CSV as input and generates a new CSV that has current (rather recent market price).

**Input CSV columns**
|Stock|Segment|Stock Code  |
|:-------------|:-------------|:-----|
|Asian Paints Ltd| Paints|ASIANPAINT.BSE |
|Bajaj Auto Ltd| Automobiles|BAJAJ-AUTO.BSE |  


**Out CSV columns**
|Stock|Segment|Stock Code|RecentPrice|OneMonthReturn|SixMonthReturn|OneYearReturn|ThreeYearReturn|FiveYearReturn|
|:-------------|:-------------|:-----|:-----|:-----|:-----|:-----|:-----|:-----|
|Asian Paints Ltd| Paints|ASIANPAINT.BSE|0.0|0.0|0.0|0.0|0.0|0.0|
|Bajaj Auto Ltd| Automobiles|BAJAJ-AUTO.BSE|0.0|0.0|0.0|0.0|0.0|0.0|
