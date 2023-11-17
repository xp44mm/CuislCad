module sector

//弦长
let chordLength r a = 2. * r * sin(a / 2.)
let getAngleFromChordLength r value = 2. * asin(value / (2. * r))


//圆心到弦的距离
let height r a = r * cos(a / 2.)
let getAngleFromHeight r value = 2. * acos(value / r)

//弧高
let bulge r a = r * (1. - cos(a / 2.))
let getAngleFromBulge r value = 2. * acos(1. - value / r)
