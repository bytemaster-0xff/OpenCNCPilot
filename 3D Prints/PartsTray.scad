
width = 40;
tray_height = 4;

rows = 1;
tape_height = 8.3;
row_margin = 1;
tape_thickness = 0.5;

part_width = 4.7;
sprocket_width = 4;
parts_margin = 3;

module sprocketPin() {
    cylinder(d1=1, d2=0.6, h=tape_thickness * 2, $fn=50);
}

difference() {
    cube([width + (2 * parts_margin),(rows) * (tape_height + row_margin) + (parts_margin * 2) + 1, tray_height]);
    for(y=[0:rows-1]) {
        row_bottom = y * (tape_height + row_margin) + parts_margin;
        translate([parts_margin,row_bottom + 1, tray_height - tape_thickness]) 
           cube([width,tape_height, 2]);    
        translate([parts_margin,row_bottom + (sprocket_width), tray_height - 3]) cube([width, part_width, 4]);
    }
}

for(y=[0:rows-1]) {
    row_bottom = y * (tape_height + row_margin) + parts_margin;
    translate([parts_margin + 4, row_bottom + 3 , (tray_height - tape_thickness)]) sprocketPin();
    translate([parts_margin + 12, row_bottom + 3 , (tray_height - tape_thickness)]) sprocketPin();
    translate([parts_margin + 20, row_bottom + 3 , (tray_height - tape_thickness)]) sprocketPin();
}

